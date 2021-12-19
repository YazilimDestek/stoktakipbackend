using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/qrtransfer")]
    [ApiController]
    public class QRTransController : BaseController
    {

        public QRTransController(StokDbContext context,
            IMapper mapper) : base(context, mapper)
        {
        }

        [HttpPost]
        public ActionResult<Result<QRTransfer>> CreateNewRequestModel([FromBody]QRTransferModel transferModel)
        {
            var response = new Result<QRTransfer>() { Meta = new Meta() };
            
            try
            {
                QrModel numbers = JsonConvert.DeserializeObject<QrModel>(((JsonElement)transferModel.QRModel).ToString());
                if(numbers != null)
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        Brand Brand = null;
                        ItemType ItemType = null;
                        ItemKind ItemKind = null;

                        try
                        {
                            if (!string.IsNullOrEmpty(numbers.Marka))
                            {
                                // yeni giriş marka
                                Brand = context.Brands.FirstOrDefault(c => c.IsDeleted == false && c.Name == numbers.Marka);

                                if (Brand == null)
                                {
                                    Brand = new Brand();
                                    Brand.Name = numbers.Marka;
                                    Brand.CreatedDateTime = DateTime.Now;
                                    Brand.CreatedUserId = GetUserID();
                                    Brand.IsDeleted = false;

                                    context.Brands.Add(Brand);
                                    context.SaveChanges();
                                }
                            }

                            if(Brand == null)
                            {
                                response.Meta.IsSuccess = false;
                                response.Meta.Error = "brand.cannot.be.created";
                                response.Meta.ErrorMessage = "Sunucu tarafında bir hata meydana geldi, tekrar deneyiniz";
                                return response;
                            }

                            if (!string.IsNullOrEmpty(numbers.Tip))
                            {
                                // yeni giriş tip
                                ItemType = context.ItemTypes.FirstOrDefault(c => c.IsDeleted == false && c.Name == numbers.Tip);

                                if (ItemType == null)
                                {
                                    ItemType = new ItemType();
                                    ItemType.Name = numbers.Tip;
                                    ItemType.CreatedDateTime = DateTime.Now;
                                    ItemType.CreatedUserId = GetUserID();
                                    ItemType.IsDeleted = false;

                                    context.ItemTypes.Add(ItemType);
                                    context.SaveChanges();
                                }
                            }

                            if (!string.IsNullOrEmpty(numbers.Tur))
                            {
                                // yeni giriş tür
                                ItemKind = context.ItemKinds.FirstOrDefault(c => c.IsDeleted == false && c.Name == numbers.Tur);

                                if (ItemKind == null)
                                {
                                    ItemKind = new ItemKind();
                                    ItemKind.Name = numbers.Tip;
                                    ItemKind.CreatedDateTime = DateTime.Now;
                                    ItemKind.CreatedUserId = GetUserID();
                                    ItemKind.IsDeleted = false;

                                    context.ItemKinds.Add(ItemKind);
                                    context.SaveChanges();
                                }
                            }

                            foreach (var serial in numbers.Seri.Where(c=> c > 0).Distinct())
                            {
                                var item = context.Items.FirstOrDefault(c => c.BrandId == Brand.Id && c.SerialNumber == serial.ToString());

                                if (item == null)
                                {
                                    item = new Item();
                                    item.CreatedDateTime = DateTime.Now;
                                    item.CreatedUserId = GetUserID();
                                    item.SerialNumber = serial.ToString();
                                    item.BrandId = Brand.Id;

                                    if (ItemType != null)
                                    {
                                        item.ItemTypeId = ItemType.Id;
                                    }

                                    if (ItemKind != null)
                                    {
                                        item.ItemKindId = ItemKind.Id;
                                    }

                                    context.Items.Add(item);
                                    context.SaveChanges();
                                }

                                // tekil transfer işlemi
                                var level = context.StockLevels.FirstOrDefault(c => c.IsDeleted == false && c.LocationId == transferModel.DestinationLocationId && c.BrandId == item.BrandId && c.SerialNumber == item.SerialNumber);

                                if (level == null)
                                {
                                    level = new StockLevel();
                                    level.LocationId = transferModel.DestinationLocationId;
                                    level.Qty = 1;
                                    level.SerialNumber = item.SerialNumber;
                                    level.BrandId = item.BrandId;
                                    level.CreatedDateTime = DateTime.Now;
                                    level.CreatedUserId = GetUserID();
                                    level.IsDeleted = false;

                                    context.StockLevels.Add(level);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    if(level.Qty == 1)
                                    {
                                        continue;
                                    }
                                }

                                int? toLocationId = (int?)null;
                                foreach (var currentLevel in context.StockLevels.Where(c => c.IsDeleted == false && c.LocationId != transferModel.DestinationLocationId && c.BrandId == item.BrandId && c.SerialNumber == item.SerialNumber).ToList())
                                {
                                    toLocationId = currentLevel.LocationId;
                                    currentLevel.Qty = 0;
                                    context.Entry(currentLevel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                    context.SaveChanges();
                                }

                                // transfer kayıt işlemi
                                var historyRecord = new StockHistory();
                                historyRecord.Barcode = item.Barcode;
                                historyRecord.BrandId = item.BrandId;
                                historyRecord.CreatedDateTime = DateTime.Now;
                                historyRecord.CreatedUserId = GetUserID();

                                if (toLocationId > 0)
                                {
                                    historyRecord.FromLocationId = toLocationId ?? 0;
                                }
                                else
                                {
                                    historyRecord.FromLocationId = transferModel.DestinationLocationId;
                                }

                                historyRecord.ToLocationId = transferModel.DestinationLocationId;
                                historyRecord.IsDeleted = false;
                                historyRecord.Qty = 1;
                                historyRecord.SerialNumber = item.SerialNumber;
                                historyRecord.TransTypeId = transferModel.TransactionTypeId;

                                context.StockHistories.Add(historyRecord);
                                context.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            dbContextTransaction.Rollback();
                            response.Meta.IsSuccess = false;
                            response.Meta.Error = "unexpected.exception : " + ex.Message;
                            response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu, tekrar deneyiniz.";
                            return response;
                        }

                        dbContextTransaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "type.mismatch";
                response.Meta.ErrorMessage = "Okutmuş olduğunuz qr kod, transfere uygun tipte veri içermiyor.";
                return response;
            }

            response.Meta.IsSuccess = true;
            return response;

        }
    }

    public class QrModel
    {
        public string Marka { get; set; }
        public string Tip { get; set; }
        public string Tur { get; set; }
        public List<long> Seri { get; set; }
    }
}
