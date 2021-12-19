using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/transfer")]
    [ApiController]
    public class TransController : BaseController
    {

        public TransController(StokDbContext context,
            IMapper mapper) : base(context, mapper)
        {
        }

        [HttpPost]
        public ActionResult<Result<Transfer>> CreateNewRequestModel([FromBody] SerialModel transferModel)
        {
            var response = new Result<Transfer>() { Meta = new Meta() };

            if(transferModel.DestinationLocationId == 0)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "destination.location.id.is.null";
                response.Meta.ErrorMessage = "Hedef lokasyon boş geçilemez";
                return response;
            }

            if(transferModel.TransactionTypeId == 0)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "transactiontypeid.is.null";
                response.Meta.ErrorMessage = "Transfer tipi boş geçilemez";
                return response;
            }

            if(transferModel.SerialNumbers == null)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "serialnumbers.is.null";
                response.Meta.ErrorMessage = "En az bir adet seri numara gönderiniz";
                return response;
            }

            if (transferModel.SerialNumbers.Count() == 0)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "serialnumbers.is.empty";
                response.Meta.ErrorMessage = "En az bir adet seri numara gönderiniz";
                return response;
            }

            try
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var serial in transferModel.SerialNumbers)
                        {
                            var item = context.Items.FirstOrDefault(c => c.IsSingular == true && c.SerialNumber == serial);
                            if(item != null)
                            {
                                // transfer işlemi
                                var level = context.StockLevels.FirstOrDefault(c => c.LocationId == transferModel.DestinationLocationId && c.BrandId == item.BrandId && c.SerialNumber == item.SerialNumber);

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
                                foreach (var currentLevel in context.StockLevels.Where(c => c.LocationId != transferModel.DestinationLocationId && c.BrandId == item.BrandId && c.SerialNumber == item.SerialNumber))
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
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        response.Meta.IsSuccess = false;
                        response.Meta.Error = ex.Message;
                        response.Meta.ErrorMessage = "Sunucu tarafından bir hata oluştu, tekrar deneyiniz.";
                        return response;
                    }

                    dbContextTransaction.Commit();
                }
                
            }
            catch (Exception ex)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = ex.Message;
                response.Meta.ErrorMessage = "Sunucu tarafından gönderdiğiniz taleple ilgili bir hata oluştu, tekrar deneyiniz.";
                return response;
            }

            response.Meta.IsSuccess = true;
            return response;
            
        }
    }

    public class SerialModel
    {
        public int DestinationLocationId { get; set; }
        public int TransactionTypeId { get; set; }
        public List<string> SerialNumbers { get; set; }
    }
}
