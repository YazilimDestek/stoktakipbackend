using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using CinigazStokService.Models.Category;
using CinigazStokService.Models.Item;
using CinigazStokService.services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CinigazStokService.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class ItemsController : BaseController
    {
        private readonly ICatagoryValidation catagoryValidation;
        private readonly IpaginationService _paginationService;

        public ItemsController(StokDbContext context,
            IMapper mapper,
            ICatagoryValidation catagoryValidation,
            IpaginationService paginationService
            ) : base(context, mapper)
        {
            this.catagoryValidation = catagoryValidation;
            this._paginationService = paginationService;
        }

        // GET api/item
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<string>> Get()
        {
            var items = context.Items.Include(c=>c.ItemType).Include(c=>c.ItemKind).Select(c => c.Name).Take(10).ToList();
            return items.ToArray();
        }

        // GET api/item/verify
        [HttpGet("verify")]
        [AllowAnonymous]
        public ActionResult<List<string>> VerifyService()
        {
            return new List<string> { "it's all good", "it works!" };
        }

        [HttpPost("search")]
        public ActionResult<Result<ItemModel>> SearchItem([FromBody] SearchItemModel model)
        {
            var Return = new Result<ItemModel>();
            Return.Meta = new Meta();

            var entities = context.Items.Include(c => c.Category).Include(c=>c.ItemKind).Include(c=>c.ItemType).Include(c => c.Brand).Where(c => !string.IsNullOrEmpty(c.SerialNumber));

            if (model.BrandId > 0)
            {
                entities = entities.Where(c => c.BrandId == model.BrandId);
            }

            if (!string.IsNullOrEmpty(model.SerialNumber))
            {
                entities = entities.Where(c => c.SerialNumber == model.SerialNumber);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                entities = entities.Where(c => c.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Barcode))
            {
                entities = entities.Where(c => c.Barcode.Equals(model.Barcode));
            }

            Return.Entities = entities.Take(10).Select(c => new ItemModel {
                ItemType = c.ItemType,
                ItemKind = c.ItemKind,
                barcode = c.Barcode,
                Brand = c.Brand,
                brandId = c.BrandId,
                Category = mapper.Map<Category, CategoryModel>(c.Category),
                categoryId = c.CategoryId ?? 0,
                historyRecords = context.StockHistories.Where(e => e.SerialNumber == c.SerialNumber && e.BrandId == c.BrandId).OrderByDescending(d => d.CreatedDateTime).Take(10).ToList(),
                isSingular = c.IsSingular,
                description = c.Description,
                name = c.Name,
                serialNumber = c.SerialNumber,
                requiredFieldValues = JsonConvert.DeserializeObject<List<RequiredFieldValues>>(c.RequiredFieldValues),
                usingVariants = c.IsUsingVariants ?? false,
                variants = context.ItemVariants.Where(f => f.IsDeleted == false && f.SerialNumber == c.SerialNumber && f.BrandId == c.BrandId).Select(f => new Variant { barcode = f.Barcode, Id = f.Id, variantParams = JsonConvert.DeserializeObject<List<VariantParams>>(f.VariantParams) }).ToList()
            }).ToList();

            Return.Meta.IsSuccess = true;
            return Return;
        }

        [HttpGet("variants/{serial}/{brandId}")]
        public ActionResult<Result<Variant>> GetVariants(string serial, int brandId)
        {
            var Return = new Result<Variant>();
            Return.Meta = new Meta();
            Return.Entities = new List<Variant>();

            try
            {
                var DbVariants = context.ItemVariants.Where(c => 
                        c.IsDeleted == false && 
                        c.SerialNumber == serial && 
                        c.BrandId == brandId
                    ).ToList();

                foreach (var variant in DbVariants)
                {
                    Return.Entities.Add(
                               new Variant
                               {
                                   Id = variant.Id,
                                   barcode = variant.Barcode,
                                   variantParams = JsonConvert.DeserializeObject<List<VariantParams>>(variant.VariantParams)
                               }
                           );
                }

                Return.Meta.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return Return;
            }

            return Return;
        }

        [HttpPost]
        public ActionResult<Result<Item>> GetByFilter([FromBody] GetItemsModel model)
        {
            var Return = new Result<Item>();
            Return.Meta = new Meta();

            try
            {
                var Items = context.Items.Include(c => c.Brand).Include(c=>c.ItemType).Include(c=>c.ItemKind).Include(c => c.Category).Where(c => !string.IsNullOrEmpty(c.SerialNumber));

                if (!string.IsNullOrEmpty(model.Name))
                {
                    Items = Items.Where(c => c.Name.Contains(model.Name));
                }

                if (model.BrandId != null && model.BrandId > 0)
                {
                    Items = Items.Where(c => c.BrandId == model.BrandId);
                }

                if (model.ItemKindId != null && model.ItemKindId > 0)
                {
                    Items = Items.Where(c => c.ItemKindId == model.ItemKindId);
                }

                if (model.ItemTypeId != null && model.ItemTypeId > 0)
                {
                    Items = Items.Where(c => c.ItemTypeId == model.ItemTypeId);
                }

                if (model.CategoryId != null && model.CategoryId > 0)
                {
                    Items = Items.Where(c => c.CategoryId == model.CategoryId);
                }

                if (!string.IsNullOrEmpty(model.SerialNumber))
                {
                    Items = Items.Where(c => c.SerialNumber == model.SerialNumber);
                }

                var pagedItems = _paginationService.PaginationRequest(model.PageIndex, model.PageCount, Items);


                Return.Entities = pagedItems.EntityQueryable.ToList();
                Return.Meta.BasePaginationModel = pagedItems.BasePaginationModel;
                Return.Meta.IsSuccess = true;
                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception : " + ex.Message;
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return Return;
            }
        }

        // GET api/item/5
        [HttpGet("{serial}/{brandId}")]
        public ActionResult<Result<ItemModel>> Get(string serial, int brandId)
        {
            var Return = new Result<ItemModel>();
            Return.Meta = new Meta();

            try
            {
                var Item = context.Items.Include(c => c.Brand).Include(c => c.Category).FirstOrDefault(c => c.BrandId == brandId && c.SerialNumber == serial);
                var Users = context.Users.ToList();

                if (Item != null)
                {
                    Return.Entity = new ItemModel();
                    Return.Entity.barcode = Item.Barcode;
                    Return.Entity.brandId = Item.BrandId;
                    Return.Entity.Brand = Item.Brand;
                    Return.Entity.Category = mapper.Map<Category, CategoryModel>(Item.Category);
                    Return.Entity.categoryId = Item.CategoryId ?? 0;
                    Return.Entity.description = Item.Description;
                    Return.Entity.brandId = Item.BrandId;
                    Return.Entity.serialNumber = Item.SerialNumber;
                    Return.Entity.isSingular = Item.IsSingular;
                    Return.Entity.name = Item.Name;
                    Return.Entity.usingVariants = Item.IsUsingVariants ?? false;
                    Return.Entity.variants = new List<Variant>();
                    Return.Entity.specifications = JsonConvert.DeserializeObject<List<Specification>>(Item.Specifications);
                    Return.Entity.requiredFieldValues = JsonConvert.DeserializeObject<List<RequiredFieldValues>>(Item.RequiredFieldValues);
                    Return.Entity.CreatedDateTime = Item.CreatedDateTime;
                    Return.Entity.CreatedUserId = Item.CreatedUserId;
                    Return.Entity.CreatedUsername = Item.CreatedUserId > 0 ? Users.FirstOrDefault(c => c.Id == Item.CreatedUserId).Username : string.Empty;
                    Return.Entity.UpdatedDateTime = Item.UpdatedDateTime;
                    Return.Entity.UpdatedUserId = Item.UpdatedUserId;
                    Return.Entity.UpdatedUsername = Item.UpdatedUserId > 0 ? Users.FirstOrDefault(c => c.Id == Item.UpdatedUserId).Username : string.Empty;

                    var DbVariants = context.ItemVariants.Where(c => 
                            c.SerialNumber == Item.SerialNumber && 
                            c.BrandId == Item.BrandId && 
                            c.IsDeleted == false
                        );

                    foreach (var variant in DbVariants)
                    {

                        Return.Entity.variants.Add(
                            new Variant
                            {
                                Id = variant.Id,
                                barcode = variant.Barcode,
                                variantParams = JsonConvert.DeserializeObject<List<VariantParams>>(variant.VariantParams)
                            }
                        );

                    }

                    Return.Meta.IsSuccess = true;
                    return Return;
                }
                else
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "record.not.found";
                    Return.Meta.ErrorMessage = "Ürün bulunamadı, silinmiş olabilir.";
                    return Return;
                }
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return Return;
            }
        }

        // POST api/item/create
        [HttpPost("create")]
        public ActionResult<Result<Item>> Create([FromBody] ItemModel request)
        {
            var Return = new Result<Item>();
            Return.Meta = new Meta();

            var IsItemAndUserValid = EnsureItemAndUserValid(request);

            if (!IsItemAndUserValid.IsSuccess)
            {
                Return.Meta = IsItemAndUserValid;
                return Return;
            }

            var response = catagoryValidation.EnsureMandotaryCatagoryFields(request);

            if (!response.Meta.IsSuccess)
            {
                return response;
            }

            var Item = new Item();

            try
            {
                if (string.IsNullOrEmpty(request.barcode))
                {
                    Item.Barcode = request.serialNumber;
                }
                else
                {
                    Item.Barcode = request.barcode;
                }

                var currentItem = context.Items.FirstOrDefault(c => c.SerialNumber == request.serialNumber && c.BrandId == request.brandId);
                if(currentItem != null)
                {
                    return new Result<Item>()
                    {
                        Meta = new Meta
                        {
                            IsSuccess = false,
                            Error = "item.already.exists",
                            ErrorMessage = request.serialNumber + " seri numarası ve seçili marka ile sistemde bir adet ürün mevcut, seri numarası çoğul olamaz"
                        }
                    };
                }
                
                Item.CategoryId = request.categoryId;
                Item.BrandId = request.brandId;
                Item.CreatedDateTime = DateTime.Now;
                Item.CreatedUserId = GetUserID();
                Item.Description = request.description;
                Item.IsSingular = request.isSingular;
                Item.IsUsingVariants = request.usingVariants;
                Item.Name = request.name;
                Item.SerialNumber = request.serialNumber;
                Item.Specifications = JsonConvert.SerializeObject(request.specifications);
                Item.RequiredFieldValues = JsonConvert.SerializeObject(request.requiredFieldValues);

                context.Items.Add(Item);
                context.SaveChanges();

                if (request.variants != null)
                {
                    if (request.usingVariants)
                    {
                        foreach (var newVariant in request.variants)
                        {
                            context.ItemVariants.Add(new ItemVariant
                            {
                                Barcode = newVariant.barcode,
                                CreatedDateTime = DateTime.Now,
                                CreatedUserId = GetUserID(),
                                IsDeleted = false,
                                SerialNumber = Item.SerialNumber,
                                BrandId = Item.BrandId,
                                VariantParams = JsonConvert.SerializeObject(newVariant.variantParams)
                            });
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu.";
                return Return;
            }

            Return.Entity = Item;
            Return.Meta.IsSuccess = true;
            return Return;
        }

        // PUT api/item/5
        [HttpPut("{id}")]
        public ActionResult<Result<Item>> Update(int id, [FromBody] ItemModel request)
        {
            var Return = new Result<Item>();
            Return.Meta = new Meta();

            var IsItemAndUserValid = EnsureItemAndUserValid(request, false);

            if (!IsItemAndUserValid.IsSuccess)
            {
                Return.Meta = IsItemAndUserValid;
                return Return;
            }

            var response = catagoryValidation.EnsureMandotaryCatagoryFields(request);
            if (!response.Meta.IsSuccess)
            {
                return response;
            }

            if (string.IsNullOrEmpty(request.serialNumber))
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "serial.is.null";
                Return.Meta.ErrorMessage = "Seri No Alanı Boş Bırakılamaz";
                return Return;
            }

            try
            {
                var item = context.Items.FirstOrDefault(c => c.BrandId == request.brandId && c.SerialNumber == request.serialNumber);

                if (item == null)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "record.not.found";
                    Return.Meta.ErrorMessage = "Kayıt bulunamadı!";
                    return Return;
                }

                item.BrandId = request.brandId;
                item.Barcode = request.barcode;
                item.CategoryId = request.categoryId;
                item.Description = request.description;
                item.IsSingular = request.isSingular;
                item.IsUsingVariants = request.usingVariants;
                item.Name = request.name;
                item.SerialNumber = request.serialNumber;
                item.Specifications = JsonConvert.SerializeObject(request.specifications);
                item.RequiredFieldValues = JsonConvert.SerializeObject(request.requiredFieldValues);
                item.UpdatedDateTime = DateTime.Now;
                item.UpdatedUserId = GetUserID();

                // ürün varyantlıysa
                if (request.usingVariants == true)
                {
                    var CurrentVariants = context.ItemVariants.Where(c => c.SerialNumber == item.SerialNumber && c.BrandId == item.BrandId).ToList();

                    if (CurrentVariants != null)
                    {
                        if (CurrentVariants.Count() > 0)
                        {
                            // gelen request içerisinde yok fakat database'de olan varyantlar.
                            var DeleteVariants = CurrentVariants.Where(c => !(request.variants.Select(d => d.Id).ToList().Contains(c.Id))).ToList();
                            if (DeleteVariants != null)
                            {
                                if (DeleteVariants.Count() > 0)
                                {
                                    var Levels = context.StockLevels.Include(c => c.ItemVariant).Where(c => c.VariantId > 0 && DeleteVariants.Select(e => e.Id).Contains(c.VariantId ?? 0)).ToList();

                                    if (Levels != null)
                                    {
                                        // silinmesi istenen varyantlardan bir veya birden fazlasında stok durumu sıfırdan büyükse silme, hata üret.
                                        if (Levels.Sum(c => c.Qty) > 0)
                                        {
                                            Return.Meta.IsSuccess = false;
                                            Return.Meta.Error = "variant.stocklevel.is.not.empty";
                                            Return.Meta.ErrorMessage = "Silmek istediğiniz varyantın stoğu bulunmaktadır!";
                                            return Return;
                                        }
                                    }

                                    // silinmesi gereken varyantları siliyoruz
                                    foreach (var deleteVariant in DeleteVariants)
                                    {
                                        deleteVariant.IsDeleted = true;
                                        context.Entry(deleteVariant).State = EntityState.Modified;
                                    }
                                    context.SaveChanges();
                                }
                            }
                        }
                    }



                    foreach (var requestVariant in request.variants)
                    {
                        var CurrentVariant = CurrentVariants.FirstOrDefault(c => c.Id == requestVariant.Id);

                        if (CurrentVariant != null)
                        {
                            CurrentVariant.VariantParams = JsonConvert.SerializeObject(requestVariant.variantParams);
                            CurrentVariant.UpdatedDateTime = DateTime.Now;
                            CurrentVariant.UpdatedUserId = GetUserID();

                            context.Entry(CurrentVariant).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                        else
                        {
                            context.ItemVariants.Add(
                                new ItemVariant
                                {
                                    Barcode = requestVariant.barcode,
                                    CreatedDateTime = DateTime.Now,
                                    CreatedUserId = GetUserID(),
                                    IsDeleted = false,
                                    SerialNumber = item.SerialNumber,
                                    BrandId = item.BrandId,
                                    VariantParams = JsonConvert.SerializeObject(requestVariant.variantParams)
                                }
                            );
                        }
                    }
                }

                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();

                Return.Meta.IsSuccess = true;
                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return Return;
            }
        }

        // DELETE api/item/5
        [HttpDelete("{serial}/{brandId}")]
        public ActionResult<Result<Item>> Delete(string serial, int brandId)
        {
            var Return = new Result<Item>();
            Return.Meta = new Meta();

            if (GetUser().ItemDelete == false)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            try
            {
                var item = context.Items.FirstOrDefault(c => c.BrandId == brandId && c.SerialNumber == serial);

                var levels = context.StockLevels.Include(c=>c.Location).Where(c => 
                                c.IsDeleted == false && 
                                c.SerialNumber == item.SerialNumber && 
                                c.BrandId == item.BrandId && 
                                c.Qty > 0
                            );

                if (levels != null)
                {
                    if(levels.Count() > 0)
                    {
                        Return.Meta.IsSuccess = false;
                        Return.Meta.Error = "item.is.in.stock";
                        Return.Meta.ErrorMessage = "Ürün silinmeden önce stoğunun sıfırlanması gerekmektedir! Depolar : " + string.Join(',', levels.Select(c => c.Location.Name).ToList());
                        return Return;
                    }
                }

                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();

                Return.Meta.IsSuccess = true;
                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.error";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return Return;
            }
        }

        private Meta EnsureItemAndUserValid(ItemModel request, bool insert = true)
        {
            var meta = new Meta();
            meta.IsSuccess = true;
            var user = GetUser();

            var itemCheck = context.Items.Where(c => c.SerialNumber == request.serialNumber && c.BrandId == request.brandId).ToList();
            if (insert == true && itemCheck.Count() > 0)
            {
                meta.IsSuccess = false;
                meta.Error = "matching.item";
                meta.ErrorMessage = "Eklemek istediğiniz ürün mevcutta bulunmaktadır.";
                return meta;
            }

            if (user.Id <= 0)
            {
                meta.IsSuccess = false;
                meta.Error = "login.required";
                meta.ErrorMessage = "Uygulamaya tekrar giriş yapınız.";
                return meta;
            }

            if (!ModelState.IsValid)
            {
                meta.IsSuccess = false;
                meta.Error = "modelstate.is.invalid";
                meta.ErrorMessage = "Göndermiş olduğunuz değerleri kontrol ediniz.";
                return meta;
            }

            if (insert ? user.ItemAdd == false : user.ItemEdit == false)
            {
                meta.IsSuccess = false;
                meta.Error = "authority.error";
                meta.ErrorMessage = "Yetkiniz Bulunmamaktadir!";
                return meta;
            }

            if (request.brandId <= 0)
            {
                meta.IsSuccess = false;
                meta.Error = "Brand.requried";
                meta.ErrorMessage = "Marka Gırmek Zorundasınız!";
                return meta;
            }

            if (request.categoryId <= 0)
            {
                meta.IsSuccess = false;
                meta.Error = "Catogory.requried";
                meta.ErrorMessage = "Kategori Gırmek Zorundasınız!";
                return meta;
            }

            if (string.IsNullOrEmpty(request.serialNumber))
            {
                meta.IsSuccess = false;
                meta.Error = "Serial.requried";
                meta.ErrorMessage = "Seri Numarası Gırmek Zorundasınız!";
                return meta;
            }

            return meta;

        }

        [HttpPost("aybstransfer")]
        public ActionResult<Result<Item>> AybsTransfer([FromBody] AybsModel request)
        {
            var Return = new Result<Item>() { Meta = new Meta() };
            
            try
            {
                var config = new Dictionary<string, string>
                {
                    { "bootstrap.servers", "localhost:9092" }
                };

                var pb = new ProducerBuilder<Null, string>(config);
                using (var producer = pb.Build())
                {
                    string text = null;

                    text = Console.ReadLine();
                    producer.ProduceAsync("message", new Message<Null, string> { Value = text });

                    producer.Flush();
                }
            }
            catch (Exception)
            {

            }

            Return.Meta.IsSuccess = true;
            return Return;
        }
    }
}
