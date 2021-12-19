using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Route("api/brand")]
    [ApiController]
    public class BrandController : BaseController
    {
        public BrandController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<Brand>> Get()
        {
            var items = context.Brands.Where(c=>c.IsDeleted == false).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<Brand>> Get(int id)
        {
            var Return = new Result<Brand>();
            Return.Meta = new Meta();

            try
            {
                var brand = context.Brands.FirstOrDefault(c => c.Id == id);
                Return.Entity = brand;
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
        public ActionResult<Result<Brand>> Post([FromBody]Brand request)
        {
            var Return = new Result<Brand>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                context.Brands.Add(request);
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
        public ActionResult<Brand> Put(int id, [FromBody]Brand request)
        {
            var item = context.Brands.FirstOrDefault(c => c.Id == id);

            item.UpdatedDateTime = DateTime.Now;
            item.UpdatedUserId = GetUserID();
            item.Name = request.Name;
            
            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return item;

        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(int id)
        {
            try
            {
                var item = context.Brands.FirstOrDefault(c => c.Id == id);
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
