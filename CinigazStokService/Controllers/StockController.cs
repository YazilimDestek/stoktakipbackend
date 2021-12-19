using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Enums;
using CinigazStokService.Handler;
using CinigazStokService.Models;
using CinigazStokService.Models.Item;
using CinigazStokService.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : BaseController
    {
        private readonly IImageHandler _imageHandler;
        private readonly IpaginationService _paginationService;

        public StockController(StokDbContext context,
            IMapper mapper,
            IImageHandler imageHandler,
            IpaginationService paginationService) : base(context, mapper)
        {
            _imageHandler = imageHandler;
            _paginationService = paginationService;
        }

        [HttpGet("currentlevels/{serialnumber}/{brandid}")]
        public ActionResult<Result<StockLevel>> GetCurrentStockLevel(string serialnumber, int brandid)
        {
            var Return = new Result<StockLevel>();
            Return.Meta = new Meta();


            var levels = context.StockLevels.Where(c => 
                    c.SerialNumber == serialnumber && 
                    c.BrandId == brandid && 
                    c.IsDeleted == false
                ).ToList();

            Return.Entities = levels;
            Return.Meta.IsSuccess = true;
            return Return;
        }

        [HttpGet("history/{serialnumber}/{brandid}")]
        public ActionResult<Result<StockHistory>> GetItemHistory(string serialnumber, int brandid)
        {
            var Return = new Result<StockHistory>();
            Return.Meta = new Meta();

            var histories = context.StockHistories.Where(c => c.SerialNumber == serialnumber && c.BrandId == brandid && c.IsDeleted == false).ToList();
            Return.Entities = histories;
            Return.Meta.IsSuccess = true;
            return Return;
        }

        [HttpPost]
        public ActionResult<Result<StockHistoryModel>> GetFiltered([FromBody] StockHistoryFilter filter)
        {
            var Return = new Result<StockHistoryModel>();
            Return.Meta = new Meta();

            try
            {
                var historyRecords = context.StockHistories.Include(c=>c.Item).Include(c=>c.Item.Brand).Where(c => c.IsDeleted == false);

                if (filter.stockStartDate != null)
                {
                    historyRecords.Where(c => c.CreatedDateTime > filter.stockStartDate);
                }

                if (filter.stockEndDate != null)
                {
                    historyRecords.Where(c => c.CreatedDateTime < filter.stockEndDate);
                }

                if (filter.fromLocationId > 0)
                {
                    historyRecords = historyRecords.Where(c => c.FromLocationId == filter.fromLocationId);
                }

                if (filter.toLocationId > 0)
                {
                    historyRecords = historyRecords.Where(c => c.ToLocationId == filter.toLocationId);
                }

                if (filter.transTypeId > 0)
                {
                    historyRecords = historyRecords.Where(c => c.TransTypeId == filter.transTypeId);
                }

                if (!string.IsNullOrEmpty(filter.barcode))
                {
                    historyRecords = historyRecords.Where(s => s.Barcode == filter.barcode);
                }
                var pagedHistoryRecord = _paginationService.PaginationRequest(filter.page, filter.pageSize, historyRecords);


                var users = context.Users.ToList();

                Return.Entities = new List<StockHistoryModel>();
                foreach (var history in pagedHistoryRecord.EntityQueryable
                    .Include(c => c.Item)
                    .Include(c => c.Item.Brand)
                    .Include(c => c.Item.Category)
                    .Include(c => c.FromLocation)
                    .Include(c =>c.FromLocation.Company)
                    .Include(c => c.ToLocation)
                    .Include(c =>c.ToLocation.Company)
                    .Include(c => c.TransType)
                    .OrderByDescending(c => c.CreatedDateTime).ToList())
                {
                    var createdUser = users.FirstOrDefault(c => c.Id == history.CreatedUserId);
                    var updatedUser = users.FirstOrDefault(c => c.Id == history.UpdatedUserId);

                    Return.Entities.Add(new StockHistoryModel
                    {
                        Barcode = history.Barcode,
                        CreatedDateTime = history.CreatedDateTime,
                        CreatedUserId = history.CreatedUserId,
                        CreatedUserName = createdUser != null ? createdUser.Username : string.Empty,
                        DocumentPath = history.DocumentPath,
                        FromLocation = history.FromLocation != null ? history.FromLocation.Name + " (" + history.FromLocation.Company.Name + ")" : string.Empty,
                        FromLocationId = history.FromLocationId,
                        Id = history.Id,
                        IsDeleted = history.IsDeleted,
                        SerialNumber = history.SerialNumber,
                        BrandId = history.BrandId,
                        Qty = history.Qty,
                        RequiredFields = string.IsNullOrEmpty(history.RequiredFields) ? new List<RequiredField>() : JsonConvert.DeserializeObject<List<RequiredField>>(history.RequiredFields),
                        ItemRequiredFields = string.IsNullOrEmpty(history.Item.RequiredFieldValues) ? new List<RequiredFieldValues>() : JsonConvert.DeserializeObject<List<RequiredFieldValues>>(history.Item.RequiredFieldValues),
                        ToLocation = history.ToLocation != null ? history.ToLocation.Name + " (" + history.ToLocation.Company.Name + ")" : string.Empty,
                        ToLocationId = history.ToLocationId,
                        TransType = history.TransType != null ? history.TransType.Name : string.Empty,
                        TransTypeId = history.TransTypeId,
                        UpdatedDateTime = history.UpdatedDateTime,
                        UpdatedUserId = history.UpdatedUserId,
                        UpdatedUserName = updatedUser != null ? updatedUser.Username : string.Empty,
                        VariantId = history.VariantId,
                        categoryName = history.Item.Category != null ? history.Item.Category.Name : string.Empty,
                        Item = mapper.Map<Item,ItemModel>(history.Item)
                    });
                }

                Return.Meta.IsSuccess = true;
                Return.Meta.BasePaginationModel = pagedHistoryRecord.BasePaginationModel;

                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return Return;
            }
        }

        [HttpPost("upload")]
        [AllowAnonymous]
        public async Task<ImageReturnModel> Uploadimage(IFormFile file)
        {
            return await _imageHandler.UploadImage(file);

        }

        [HttpGet("stockmovementdetailcounts")]
        [AllowAnonymous]
        public ActionResult<Result<StockHistoryCountModel>> GetStockCounts()
        {
            var Return = new Result<StockHistoryCountModel>();
            Return.Meta = new Meta();

            try
            {
                var timeoutRecords = context.Timeouts.Where(t => t.IsDeleted == false).ToList();

                var record = context.StockHistories.Include(c => c.Item)
                        .Include(c => c.Item.Brand)
                        .Include(c => c.Item.Category)
                        .Include(c => c.Item.ItemKind)
                        .Include(c => c.Item.ItemType)
                        .Include(x => x.FromLocation)
                        .Include(x => x.Variant)
                        .Include(x => x.TransType)
                        .Include(x => x.ToLocation)
                    .Where(s =>
                    s.IsDeleted == false).ToList();

                var dashboardStatistics = new StockHistoryCountModel();


                if (record.Any())
                {
                    var todayStockRecords = record.Where(s => s.CreatedDateTime.Date == DateTime.Today.Date).ToList();

                    if (todayStockRecords.Any())
                    {
                        var rippingCount = todayStockRecords.Count(s => s.TransTypeId == 2);

                        var placementCount = todayStockRecords.Count(s => s.TransTypeId == 3);

                        dashboardStatistics.PlacementCount = placementCount;
                        dashboardStatistics.RippingCount = rippingCount;
                        dashboardStatistics.TotalStockMovementCount = todayStockRecords.Count();

                    }


                    var lastTwentyDaysRecords = record
                        .Where(x => (DateTime.Today - x.CreatedDateTime).Days <= 20)
                        .GroupBy(x => x.CreatedDateTime.Date)
                        .Select(x => new LastTwentyDays
                        {
                            Date = x.Key,
                            Count = x.Count()

                        }).ToList();


                    dashboardStatistics.LastTwentyDays = lastTwentyDaysRecords;


                    if (timeoutRecords.Any())
                    {

                        var lastRecordEachItems = record
                            .GroupBy(x => x.Item, (x, y) => new
                            {
                                history = y.OrderByDescending(z => z.CreatedDateTime).FirstOrDefault(),

                            });

                        var timeoutEachItem = lastRecordEachItems
                            .Where(x => timeoutRecords.Any(t => t.TransTypeId == x.history.TransTypeId && 
                                                                t.LocationId == x.history.ToLocationId && 
                                                                t.CategoryId == x.history.Item.CategoryId && 
                                                                ((DateTime.Today - x.history.CreatedDateTime).Days > t.Days))
                                )
                                .Select(x => new StockHistoryModel { 
                                    Id = x.history.Id, 
                                    Brand = mapper.Map<Brand,BrandModel>(x.history.Item.Brand), 
                                    Item = mapper.Map<Item,ItemModel>(x.history.Item),
                                    SerialNumber = x.history.Item.SerialNumber,
                                    Barcode = x.history.Barcode,
                                    BrandId = x.history.BrandId,
                                    categoryName = x.history.Item.Category.Name,
                                    CreatedDateTime = x.history.CreatedDateTime,
                                    CreatedUserId = x.history.CreatedUserId,
                                    CreatedUserName = context.Users.FirstOrDefault(d=>d.Id == x.history.CreatedUserId).Username,
                                    DocumentPath = x.history.DocumentPath,
                                    FromLocation = x.history.FromLocation.Name,
                                    FromLocationId = x.history.FromLocationId,
                                    IsDeleted = x.history.IsDeleted,
                                    ItemName = x.history.Item.Name,
                                    ItemRequiredFields = JsonConvert.DeserializeObject<List<RequiredFieldValues>>(x.history.Item.RequiredFieldValues),
                                    Qty = x.history.Qty,
                                    RequiredFields = JsonConvert.DeserializeObject<List<RequiredField>>(x.history.RequiredFields),
                                    ToLocation = x.history.ToLocation.Name,
                                    ToLocationId = x.history.ToLocationId,
                                    TransType = x.history.TransType.Name,
                                    TransTypeId = x.history.TransTypeId,
                                    UpdatedDateTime = x.history.UpdatedDateTime,
                                    UpdatedUserId = x.history.UpdatedUserId,
                                    VariantId = x.history.VariantId,
                                    VariantParams = x.history.Variant != null ? JsonConvert.DeserializeObject<List<VariantParams>>(x.history.Variant.VariantParams) : new List<VariantParams>(),
                                    ColorAfterTimeout = timeoutRecords.FirstOrDefault(t => t.TransTypeId == x.history.TransTypeId &&
                                                                    t.LocationId == x.history.ToLocationId &&
                                                                    t.CategoryId == x.history.Item.CategoryId &&
                                                                    ((DateTime.Today - x.history.CreatedDateTime).Days > t.Days)).ColorAfterTimeout,
                                    ColorAfterTransfer = timeoutRecords.FirstOrDefault(t => t.TransTypeId == x.history.TransTypeId &&
                                                                    t.LocationId == x.history.ToLocationId &&
                                                                    t.CategoryId == x.history.Item.CategoryId &&
                                                                    ((DateTime.Today - x.history.CreatedDateTime).Days > t.Days)).ColorAfterTransfer
                                }
                            )
                            .ToList();

                        timeoutEachItem.ForEach(x =>
                            x.CreatedUserName = context.Users.FirstOrDefault(c => c.Id == x.CreatedUserId).Username);

                        dashboardStatistics.TimeoutItems = timeoutEachItem;
                        dashboardStatistics.timeoutCount = timeoutEachItem.Count;
                    }
                }

                Return.Entity = dashboardStatistics;
                Return.Meta.IsSuccess = true;
                return Return;

            }
            catch (Exception e)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return Return;
            }

        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<Result<StockHistoryModel>> Get(int id)
        {
            var Return = new Result<StockHistoryModel>();
            Return.Meta = new Meta();

            try
            {
                var record = context.StockHistories
                    .Include(c => c.FromLocation)
                    .Include(c => c.FromLocation.Company)
                    .Include(c => c.ToLocation)
                    .Include(c => c.ToLocation.Company)
                    .Include(c => c.TransType)
                    .Include(c => c.Item)
                    .Include(c => c.Variant)
                    .Include(c => c.Item.Category)
                    .Include(c => c.Item.Brand)
                    .FirstOrDefault(c => c.Id == id);

                Return.Entity = mapper.Map<StockHistory, StockHistoryModel>(record);

                var createdUserName = context.Users.FirstOrDefault(c => c.Id == record.CreatedUserId).Username;

                Return.Entity.CreatedUserName = createdUserName;

                if (record.UpdatedUserId != null)
                {
                    var updatedUserName = context.Users.FirstOrDefault(u => u.Id == record.UpdatedUserId).Username;
                    Return.Entity.UpdatedUserName = updatedUserName;
                }

                Return.Meta.IsSuccess = true;
                return Return;

            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return Return;
            }
        }

        [HttpPost("move")]
        public ActionResult<Result<StockLevel>> MoveStock([FromBody] MoveRequestModel MoveRequest)
        {
            var Return = new Result<StockLevel>();
            Return.Meta = new Meta();

            if (GetUser().HistoryAdd == false)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            try
            {
                var Item = context.Items.FirstOrDefault(c => c.BrandId == MoveRequest.BrandId && c.SerialNumber == MoveRequest.SerialNumber);
                if (Item == null)
                {
                    // ÜRÜN BULUNAMADIYSA HATA
                    Return.Meta.Error = "item.not.found";
                    Return.Meta.ErrorMessage = "Ürün bulunamadı!";
                    Return.Meta.IsSuccess = false;
                    return Return;
                }

                Location FromLocation = null;
                if (MoveRequest.FromLocationId != null && MoveRequest.FromLocationId > 0)
                {
                    FromLocation = context.Locations.FirstOrDefault(c => c.IsDeleted == false && c.Id == MoveRequest.FromLocationId);
                }

                if (Item.IsSingular == false)
                {
                    // ÇOĞUL ÜRÜN (plural)
                    if (MoveRequest.FromLocationId == 0 || MoveRequest.FromLocationId == null)
                    {
                        Return.Meta.Error = "item.is.plural.fromlocationid.required";
                        Return.Meta.ErrorMessage = "Ürün tekil değil, kaynak lokasyonu seçiniz seçiniz!";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }

                    if (FromLocation == null && MoveRequest.TransTypeId != TransTypeEnum.Yeni.GetHashCode())
                    {
                        // ÇOĞUL ÜRÜNSE, YENİ DEĞİLSE VE GELİŞ LOKASYONU YOKSA HATA!
                        Return.Meta.Error = "item.is.plural.fromlocationid.required";
                        Return.Meta.ErrorMessage = "Ürün tekil değil, kaynak lokasyon tanımlı değil!";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }
                }
                else
                {
                    // TEKİL ÜRÜN (singular)
                    var CurrentLevel = context.StockLevels.Include(c => c.Location).FirstOrDefault(c => c.BrandId == MoveRequest.BrandId && c.SerialNumber == MoveRequest.SerialNumber);

                    if (MoveRequest.TransTypeId != TransTypeEnum.Yeni.GetHashCode()
                        && MoveRequest.TransTypeId != TransTypeEnum.GayiptenGetir.GetHashCode()
                        && MoveRequest.TransTypeId != TransTypeEnum.GayipAyir.GetHashCode())
                    {
                        // YENİ DEĞİL, GELİŞ LOKASYONU KONTROLÜ SAĞLAYALIM.
                        if (CurrentLevel == null)
                        {
                            Return.Meta.Error = "fromlocation.not.found";
                            Return.Meta.ErrorMessage = "Sadece stokta olan ürünler transfer edilebilir, lütfen 'Yeni' veya 'Gayipten Getir' işlemi yaparak önce stok giriniz";
                            Return.Meta.IsSuccess = false;
                            return Return;
                        }
                    }
                }

                ItemVariant Variant = null;
                if (MoveRequest.VariantId > 0)
                {
                    Variant = context.ItemVariants.FirstOrDefault(c => c.IsDeleted == false && c.Id == MoveRequest.VariantId);
                    if (Variant == null)
                    {
                        Return.Meta.Error = "variant.not.found";
                        Return.Meta.ErrorMessage = "Varyant bulunamadı!";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }
                }

                var Levels = context.StockLevels.Where(c => c.BrandId == MoveRequest.BrandId && c.SerialNumber == MoveRequest.SerialNumber);
                if (Levels != null && Item.IsSingular == true)
                {
                    if (Levels.Sum(c => c.Qty) > 1)
                    {
                        // ürün tekilse ve depolarda birden fazla bulunduysa hata var demektir!
                        Return.Meta.Error = "item.is.singular.but.found.in.multiple.locations";
                        Return.Meta.ErrorMessage = "Ürün tekil olarak tanımlanmış fakat birden fazla bulunmaktadır. Bilgi işlem birimi ile iletişime geçiniz.";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }
                }

                var Location = context.Locations.FirstOrDefault(c => c.Id == MoveRequest.LocationId);
                if (Location == null)
                {
                    Return.Meta.Error = "location.not.found";
                    Return.Meta.ErrorMessage = "Lokasyon bulunamadı!";
                    Return.Meta.IsSuccess = false;
                    return Return;
                }

                var TransType = context.TransTypes.FirstOrDefault(c => c.Id == MoveRequest.TransTypeId);
                if (TransType == null)
                {
                    Return.Meta.Error = "transtype.not.found";
                    Return.Meta.ErrorMessage = "Transfer tipi bulunamadı!";
                    Return.Meta.IsSuccess = false;
                    return Return;
                }

                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (Levels != null)
                        {
                            // ürünün daha önceden lokasyon adet kaydı oluşmuş.
                            StockLevel CurrentLevel = null;

                            if (FromLocation != null)
                            {
                                // geliş lokasyonu boş değilse, geliş lokasyonu kaydını alalım.
                                CurrentLevel = Levels.FirstOrDefault(c => c.LocationId == FromLocation.Id);
                            }
                            else
                            {
                                // geliş lokasyonu boşsa stokta olduğu kaydı alalım.
                                CurrentLevel = Levels.FirstOrDefault(c => c.Qty > 0);
                            }

                            if (CurrentLevel == null)
                            {
                                // mevcut bir depoda stok kaydı yok, oluşturalım.
                                CurrentLevel = new StockLevel
                                {
                                    SerialNumber = Item.SerialNumber,
                                    BrandId = Item.BrandId,
                                    VariantId = Variant != null ? Variant.Id : (int?)null,
                                    Barcode = Variant != null ? Variant.Barcode : Item.Barcode,
                                    CreatedDateTime = DateTime.Now,
                                    CreatedUserId = GetUserID(),
                                    IsDeleted = false,
                                    LocationId = Location.Id,
                                    Qty = Item.IsSingular == true ? 1 : MoveRequest.Qty
                                };
                                context.StockLevels.Add(CurrentLevel);
                            }
                            else
                            {
                                // mevcut depoda stok kaydı var
                                if (CurrentLevel.Qty == 0)
                                {
                                    if (MoveRequest.TransTypeId != TransTypeEnum.Yeni.GetHashCode() && MoveRequest.TransTypeId != TransTypeEnum.GayiptenGetir.GetHashCode())
                                    {
                                        // kaynak depoda stok sıfır, hata verelim.
                                        Return.Meta.Error = "current.level.is.zero";
                                        Return.Meta.ErrorMessage = "Transfer yapılacak ürünün stok durumu sıfır";
                                        Return.Meta.IsSuccess = false;
                                        return Return;
                                    }
                                }
                                else if (CurrentLevel.Qty > 1 && Item.IsSingular == true)
                                {
                                    // kaynak depoda stok birin üzerinde, fakat ürün tekil. hata verelim.
                                    Return.Meta.Error = "item.is.singular.stocklevel.over.one";
                                    Return.Meta.ErrorMessage = "Ürün tekil girilmiş fakat stok seviyesi 1 adetin üzerinde";
                                    Return.Meta.IsSuccess = false;
                                    return Return;
                                }

                                if (Item.IsSingular == true)
                                {
                                    // ürün tekil, kaynak depoda mevcut.
                                    // lokasyon değiştirelim.
                                    CurrentLevel.LocationId = MoveRequest.LocationId;
                                    context.Entry(CurrentLevel).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                                else
                                {
                                    // ürün tekil değil,
                                    // kaynak depodan stok eksiltme yapalım.
                                    CurrentLevel.Qty = CurrentLevel.Qty - MoveRequest.Qty;
                                    if (CurrentLevel.Qty < 0)
                                    {
                                        Return.Meta.Error = "current.level.is.less.than.movement.stock";
                                        Return.Meta.ErrorMessage = "Depodaki mevcut stok miktarı, transfer etmek istenilen stoğu karşılamakta yetersiz.";
                                        Return.Meta.IsSuccess = false;
                                        return Return;
                                    }
                                    context.Entry(CurrentLevel).State = EntityState.Modified;

                                    // hedef depoya stok ekleme yapalım.
                                    var ToStockLevel = context.StockLevels.FirstOrDefault(c => 
                                            c.LocationId == MoveRequest.LocationId && 
                                            c.SerialNumber == Item.SerialNumber && 
                                            c.BrandId == Item.BrandId
                                        );

                                    if (ToStockLevel == null)
                                    {
                                        ToStockLevel = new StockLevel
                                        {
                                            Barcode = Variant != null ? Variant.Barcode : Item.Barcode,
                                            CreatedDateTime = DateTime.Now,
                                            CreatedUserId = GetUserID(),
                                            IsDeleted = false,
                                            SerialNumber = Item.SerialNumber,
                                            BrandId = Item.BrandId,
                                            LocationId = MoveRequest.LocationId,
                                            Qty = MoveRequest.Qty
                                        };

                                        if (Variant != null)
                                        {
                                            ToStockLevel.VariantId = Variant.Id;
                                        }
                                        context.StockLevels.Add(ToStockLevel);
                                        Return.Entity = ToStockLevel;
                                    }
                                    else
                                    {
                                        ToStockLevel.Qty = ToStockLevel.Qty + MoveRequest.Qty;
                                        context.Entry(ToStockLevel).State = EntityState.Modified;
                                        Return.Entity = ToStockLevel;
                                    }

                                    context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            // ürünün daha önce bir stok kaydı oluşmamış, levels boş.
                            var ToStockLevel = new StockLevel
                            {
                                Barcode = Variant != null ? Variant.Barcode : Item.Barcode,
                                CreatedDateTime = DateTime.Now,
                                CreatedUserId = GetUserID(),
                                IsDeleted = false,
                                SerialNumber = Item.SerialNumber,
                                BrandId = Item.BrandId,
                                LocationId = MoveRequest.LocationId,
                                Qty = MoveRequest.Qty,
                                VariantId = Variant.Id
                            };

                            context.StockLevels.Add(ToStockLevel);
                            Return.Entity = ToStockLevel;
                        }

                        // TRANSFER KAYIT EDİLDİ, STOCKHISTORY EKLE
                        var HistoryRecord = new StockHistory
                        {
                            Barcode = Variant != null ? Variant.Barcode : Item.Barcode,
                            CreatedDateTime = DateTime.Now,
                            CreatedUserId = GetUserID(),
                            FromLocationId = FromLocation != null ? FromLocation.Id : 0,
                            IsDeleted = false,
                            SerialNumber = Item.SerialNumber,
                            BrandId = Item.BrandId,
                            VariantId = Variant != null ? Variant.Id : (int?)null,
                            TransTypeId = MoveRequest.TransTypeId,
                            ToLocationId = MoveRequest.LocationId,
                            RequiredFields = JsonConvert.SerializeObject(MoveRequest.RequiredFieldValues),
                            DocumentPath = MoveRequest.documentPath,
                            Qty = MoveRequest.Qty
                        };
                        context.StockHistories.Add(HistoryRecord);
                        context.SaveChanges();

                        dbContextTransaction.Commit();


                        Return.Meta.IsSuccess = true;
                        return Return;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();

                        Return.Meta.Error = "unexpected.exception";
                        Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }
                }
            }
            catch (Exception ex)
            {
                Return.Meta.Error = "unexpected.exception";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                Return.Meta.IsSuccess = false;
                return Return;
            }
        }

        [HttpPost("setlevel")]
        public ActionResult<Result<string>> SetLevel([FromBody] SetLevelRequest request)
        {
            var Return = new Result<string>();
            Return.Meta = new Meta();

            if (GetUser().HistoryAdd == false)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            var Levels = context.StockLevels.Include(c => c.Item).Where(c => c.IsDeleted == false && c.SerialNumber == request.SerialNumber && c.BrandId == request.BrandId);

            if (request.VariantId > 0)
            {
                Levels = Levels.Where(c => c.VariantId == request.VariantId);
            }

            // lokasyon tanımlanmışsa
            if (request.LocationId > 0)
            {
                var Locationlevel = Levels.FirstOrDefault(c => c.LocationId == request.LocationId);
                if (Locationlevel != null)
                {
                    // ürün tekil ve adet birden fazla girilmiş
                    if (Locationlevel.Item.IsSingular == true && request.Level > 1)
                    {
                        Return.Meta.Error = "item.is.singular";
                        Return.Meta.ErrorMessage = "Tekil ürünler birden fazla olamazlar";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }

                    // ürün tekil ve depolarda 1 adetten fazla bulunuyor.
                    if (Locationlevel.Item.IsSingular == true && Levels.Sum(c => c.Qty) > 1)
                    {
                        Return.Meta.Error = "item.is.singular.but.found.more.than.one";
                        Return.Meta.ErrorMessage = "Tekil ürünler birden fazla olamazlar";
                        Return.Meta.IsSuccess = false;
                        return Return;
                    }

                    // ürün tekil, lokasyonda değer girilmiş, diğer tanımlı depolarda sıfırlayalım.
                    if (Locationlevel.Item.IsSingular == true && request.LocationId > 0 && request.Level == 1)
                    {
                        foreach (var level in Levels)
                        {
                            level.UpdatedDateTime = DateTime.Now;
                            level.UpdatedUserId = GetUserID();
                            level.Qty = 0;
                            context.Entry(level).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }

                    Locationlevel.Qty = request.Level;
                    Locationlevel.UpdatedDateTime = DateTime.Now;
                    Locationlevel.UpdatedUserId = GetUserID();
                    context.Entry(Locationlevel).State = EntityState.Modified;
                    context.SaveChanges();

                    Return.Meta.IsSuccess = true;
                    return Return;
                }
            }
            else
            {
                Return.Meta.Error = "locationid.is.null";
                Return.Meta.ErrorMessage = "Lütfen lokasyon seçiniz!";
                Return.Meta.IsSuccess = false;
                return Return;
            }

            return Return;
        }
    }
}
