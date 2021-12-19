using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using CinigazStokService.Models.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : BaseController
    {
        public CategoryController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        //[AllowAnonymous]
        public ActionResult<Result<CategoryListModel>> Get()
        {

            var Return = new Result<CategoryListModel>();
            Return.Meta = new Meta();

            try
            {
                Return.Entities = new List<CategoryListModel>();

                var categories = context.Categories.Where(c => c.IsDeleted == false).OrderBy(c => c.Name).ToList();
                var itemCounts = context.Items.GroupBy(c => c.CategoryId).Select(c => new { key = c.Key, count = c.Count() });
                var mandatoryFields = context.CategoryFields.Where(c => categories.Select(d => d.Id).Contains(c.CategoryId)).ToList();

                foreach (var category in categories)
                {
                    var subcategoryIds = GetAllSubCategoryIds(category.Id);
                    var currentStock = itemCounts.FirstOrDefault(c => c.key == category.Id);
                    var subCatsStock = itemCounts.Where(c => subcategoryIds.Contains(c.key ?? 0));

                    var statusText = string.Empty;
                    var topCategoryName = string.Empty;

                    try
                    {
                        topCategoryName = category.TopCategoryId > 0 ? categories.FirstOrDefault(c => c.Id == category.TopCategoryId).Name : "";
                    }
                    catch (Exception ex)
                    {

                    }

                    // ALT KATEGORİLER İLE BİRLİKTE TOPLAMLAR
                    var sumStockCount = currentStock != null ? currentStock.count : 0;
                    sumStockCount = subCatsStock != null ? sumStockCount + subCatsStock.Sum(c => c.count) : sumStockCount;

                    if (currentStock == null)
                    {
                        statusText = "STOK YOK";
                    }
                    else if (currentStock.count == 0)
                    {
                        statusText = "STOK YOK";
                    }
                    else if (currentStock.count < category.MinStockCount)
                    {
                        statusText = "STOK AZALDI";
                    }
                    else if (currentStock.count > category.MaxStockCount)
                    {
                        statusText = "STOK FAZLA";
                    }
                    else
                    {
                        statusText = "STOK YETERLİ";
                    }

                    var fields = mandatoryFields.Where(c => c.CategoryId == category.Id);

                    Return.Entities.Add(new CategoryListModel
                    {
                        currentStock = currentStock == null ? 0 : currentStock.count,
                        Id = category.Id,
                        currentStatus = statusText,
                        name = category.Name,
                        topCategoryName = topCategoryName,
                        mandatoryFields = fields != null ? fields.Select(c =>
                        new CategoryFieldModel
                        {
                            Id = c.Id,
                            isMandatory = c.IsMandatory,
                            isRequiredOnDefinition = c.IsRequiredOnDefinition,
                            isRequiredOnMove = c.IsRequiredOnMove,
                            name = c.Name,
                            valueType = c.ValueType
                        }).ToList() : new List<CategoryFieldModel>()
                    });
                }

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

        private List<int> GetAllSubCategoryIds(int CategoryId)
        {
            var Return = new List<int>();

            var subCategories = context.Categories.Where(c => c.TopCategoryId == CategoryId).ToList();

            foreach (var item in subCategories)
            {
                Return.Add(item.Id);
                Return.AddRange(GetAllSubCategoryIds(item.Id));
            }

            return Return;
        }

        [HttpGet("{id}")]
        public ActionResult<Result<CategoryModel>> Get(int id)
        {
            var Return = new Result<CategoryModel>();
            Return.Meta = new Meta();

            try
            {
                var category = context.Categories.FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
                var fields = context.CategoryFields.Where(c => c.IsDeleted == false && c.CategoryId == category.Id);

                Return.Entity = new CategoryModel();
                Return.Entity.Id = category.Id;
                Return.Entity.abysTypeCode = category.AbysTypeCode;
                Return.Entity.description = category.Description;
                Return.Entity.maxStockCount = category.MaxStockCount;
                Return.Entity.maxStockWarningColor = category.MaxStockWarningColor;
                Return.Entity.minStockCount = category.MinStockCount;
                Return.Entity.minStockWarningColor = category.MinStockWarningColor;
                Return.Entity.name = category.Name;
                Return.Entity.outOfStockWarningColor = category.OutOfStockWarningColor;
                Return.Entity.topCategoryId = category.TopCategoryId;
                Return.Entity.createdDateTime = category.CreatedDateTime;

                try
                {
                    Return.Entity.TopCategory = category.TopCategoryId > 0 ? context.Categories.FirstOrDefault(c => c.Id == category.TopCategoryId) : null;
                    if (category.CreatedUserId > 0)
                    {
                        Return.Entity.createdUser = context.Users.FirstOrDefault(c => c.Id == category.CreatedUserId).Username;
                    }
                    if (category.UpdatedUserId > 0)
                    {
                        Return.Entity.updatedUser = context.Users.FirstOrDefault(c => c.Id == category.UpdatedUserId).Username;
                        Return.Entity.updatedDateTime = category.UpdatedDateTime ?? new DateTime();
                    }
                }
                catch (Exception)
                {
                }

                Return.Entity.mandatoryFields = new List<CategoryFieldModel>();
                foreach (var field in fields)
                {
                    Return.Entity.mandatoryFields.Add(new CategoryFieldModel
                    {
                        isMandatory = field.IsMandatory,
                        isRequiredOnDefinition = field.IsRequiredOnDefinition,
                        isRequiredOnMove = field.IsRequiredOnMove,
                        name = field.Name,
                        valueType = field.ValueType,
                        Id = field.Id
                    });
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

        [HttpGet("fields/{categoryid}/{requiredonmove}/{requiredoncreate}")]
        public ActionResult<Result<CategoryFieldModel>> GetRequiredFields(int categoryid, bool requiredonmove = false, bool requiredoncreate = false)
        {
            var Return = new Result<CategoryFieldModel>();
            Return.Meta = new Meta();

            try
            {
                var fields = context.CategoryFields.Where(c => c.IsDeleted == false && c.CategoryId == categoryid);

                if (requiredonmove == true)
                {
                    fields = fields.Where(c => c.IsRequiredOnMove == true);
                }

                if (requiredoncreate == true)
                {
                    fields = fields.Where(c => c.IsRequiredOnDefinition == true);
                }

                Return.Entities = fields.Select(c =>
                    new CategoryFieldModel
                    {
                        Id = c.Id,
                        isMandatory = c.IsMandatory,
                        isRequiredOnDefinition = c.IsRequiredOnDefinition,
                        isRequiredOnMove = c.IsRequiredOnMove,
                        name = c.Name,
                        valueType = c.ValueType
                    }).ToList();

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

        [HttpPost]
        public ActionResult<Result<Category>> Post([FromBody] CategoryModel request)
        {
            var Return = new Result<Category>();
            Return.Meta = new Meta();

            if (GetUser().CategoryAdd == false)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            try
            {
                request.Id = 0;

                var Category = new Category();
                Category.CreatedDateTime = DateTime.Now;
                Category.CreatedUserId = GetUserID();
                Category.Name = request.name;
                Category.AbysTypeCode = request.abysTypeCode;
                Category.Description = request.description;
                Category.IsDeleted = false;
                Category.MaxStockCount = request.maxStockCount;
                Category.MaxStockWarningColor = request.maxStockWarningColor;
                Category.MinStockCount = request.minStockCount;
                Category.MinStockWarningColor = request.minStockWarningColor;
                Category.OutOfStockWarningColor = request.outOfStockWarningColor;
                Category.TopCategoryId = request.topCategoryId;

                context.Categories.Add(Category);
                context.SaveChanges();
                if (request.mandatoryFields != null)
                {
                    foreach (var field in request.mandatoryFields)
                    {
                        var newField = new CinigazStokEntity.CategoryField
                        {
                            CategoryId = Category.Id,
                            CreatedDateTime = DateTime.Now,
                            CreatedUserId = GetUserID(),
                            IsDeleted = false,
                            IsMandatory = field.isMandatory,
                            IsRequiredOnMove = field.isRequiredOnMove,
                            IsRequiredOnDefinition = field.isRequiredOnDefinition,
                            Name = field.name,
                            ValueType = field.valueType
                        };

                        context.CategoryFields.Add(newField);
                    }
                }



                context.SaveChanges();

                Return.Meta.IsSuccess = true;
                Return.Entity = Category;
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

        [HttpPut("{id}")]
        public ActionResult<Result<Category>> Put(int id, [FromBody] CategoryModel request)
        {
            var Return = new Result<Category>();
            Return.Meta = new Meta();

            if (GetUser().CategoryAEdit == false)
            {

                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            try
            {
                var item = context.Categories.FirstOrDefault(c => c.Id == id);


                item.UpdatedDateTime = DateTime.Now;
                item.UpdatedUserId = GetUserID();
                item.AbysTypeCode = request.abysTypeCode;
                item.Description = request.description;
                item.MaxStockCount = request.maxStockCount;
                item.MaxStockWarningColor = request.maxStockWarningColor;
                item.MinStockCount = request.minStockCount;
                item.MinStockWarningColor = request.minStockWarningColor;
                item.Name = request.name;
                item.OutOfStockWarningColor = request.outOfStockWarningColor;

                if (request.topCategoryId == item.Id)
                {
                    item.TopCategoryId = 0;
                }
                else
                {
                    item.TopCategoryId = request.topCategoryId;
                }

                context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChanges();


                // DATABASE'de OLAN FAKAT GELEN ALANLAR İÇERİSİNDE OLMAYAN VARSA, SİLELİM.
                var dbFields = context.CategoryFields.Where(c => c.IsDeleted == false && c.CategoryId == item.Id);
                foreach (var dbField in dbFields)
                {
                    if (request.mandatoryFields.FirstOrDefault(c => c.Id == dbField.Id) == null)
                    {
                        dbField.IsDeleted = true;
                        context.Entry(dbField).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                context.SaveChanges();

                // GELEN ALANLARI KONTROL EDELİM, DATABASE'DE OLANLARI GÜNCELLEYELİM, OLMAYANLARI EKLEYELİM.
                foreach (var requestField in request.mandatoryFields)
                {
                    if (requestField.Id == 0 || requestField.Id == null)
                    {
                        context.CategoryFields.Add(new CinigazStokEntity.CategoryField
                        {
                            IsMandatory = requestField.isMandatory,
                            IsRequiredOnDefinition = requestField.isRequiredOnDefinition,
                            IsRequiredOnMove = requestField.isRequiredOnMove,
                            Name = requestField.name,
                            ValueType = requestField.valueType,
                            CategoryId = item.Id
                        });
                    }
                    else
                    {
                        var dbField = dbFields.FirstOrDefault(c => c.Id == requestField.Id);
                        if (dbField != null)
                        {
                            dbField.IsMandatory = requestField.isMandatory;
                            dbField.IsRequiredOnDefinition = requestField.isRequiredOnDefinition;
                            dbField.IsRequiredOnMove = requestField.isRequiredOnMove;
                            dbField.Name = requestField.name;
                            dbField.ValueType = requestField.valueType;
                            dbField.CategoryId = item.Id;

                            context.Entry(dbField).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                }

                context.SaveChanges();

                Return.Meta.IsSuccess = true;
                Return.Entity = item;
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

        [HttpDelete("{id}")]
        public ActionResult<Result<Category>> Delete(int id)
        {
            var Return = new Result<Category>();
            Return.Meta = new Meta();
            if (GetUser().CategoryDelete == false)
            {

                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.Error = "Yetkiniz Bulunmamaktadir!";
                return Return;
            }

            try
            {
                var item = context.Categories.FirstOrDefault(c => c.Id == id);
                item.IsDeleted = true;
                context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
    }
}
