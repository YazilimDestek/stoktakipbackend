using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : BaseController
    {
        public LocationController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<Location>> Get()
        {
            var items = context.Locations.Where(c => c.IsDeleted == false).Include(c => c.LocationType).Include(c => c.Company).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<Location>> Get(int id)
        {
            var Return = new Result<Location>();
            Return.Meta = new Meta();

            try
            {
                var location = context.Locations.Include(c => c.LocationType).Include(c => c.Company).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
                Return.Entity = location;
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

        [HttpGet("locationdetail/{id}")]
        public ActionResult<Result<CategoryBasedLocationModel>> GetCategoryBasedLocationDetail(int id)
        {
            var Return = new Result<CategoryBasedLocationModel>();
            Return.Meta = new Meta();

            try
            {

                var stockLevel = context.StockLevels
                    .Include(s => s.Item)
                    .Include(s => s.Item.Category)
                    .Where(s => 
                            s.LocationId == id && 
                            s.IsDeleted == false && 
                            !string.IsNullOrEmpty(s.SerialNumber) && 
                            s.BrandId > 0 && 
                            s.Item.CategoryId > 0
                        ).ToList();
                 
                if (stockLevel.Any())
                {
                    var categoryBasedLevelInLocation = stockLevel.GroupBy(x => x.Item.CategoryId, (x, y) => new CategoryBasedLocationModel
                    {
                        CategoryName = y.Select(z => z.Item.Category.Name).FirstOrDefault(),

                        ItemCount = y.Sum(z => z.Qty),

                    }).ToList();

                    Return.Entities = categoryBasedLevelInLocation;
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

        [HttpPost]
        public ActionResult<Result<Location>> Post([FromBody]Location request)
        {
            var Return = new Result<Location>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                context.Locations.Add(request);
                context.SaveChanges();

                Return.Meta.IsSuccess = true;
                Return.Entity = request;
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
        public ActionResult<Location> Put(int id, [FromBody]Location request)
        {
            var item = context.Locations.FirstOrDefault(c => c.Id == id);

            item.UpdatedDateTime = DateTime.Now;
            item.UpdatedUserId = GetUserID();
            item.Name = request.Name;
            item.LocationTypeId = request.LocationTypeId;
            item.CompanyId = request.CompanyId;

            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return item;

        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(int id)
        {
            try
            {
                var item = context.Locations.FirstOrDefault(c => c.Id == id);
                item.IsDeleted = true;
                context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
