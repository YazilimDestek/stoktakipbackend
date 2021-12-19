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
    [Route("api/company")]
    [ApiController]
    public class CompanyController : BaseController
    {
        public CompanyController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<Company>> Get()
        {
            var items = context.Companies.Where(c=>c.IsDeleted == false).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<Company>> Get(int id)
        {
            var Return = new Result<Company>();
            Return.Meta = new Meta();

            try
            {
                var record = context.Companies.FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
                Return.Entity = record;
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
        public ActionResult<Result<Company>> Post([FromBody]Company request)
        {
            var Return = new Result<Company>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                context.Companies.Add(request);
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
        public ActionResult<Company> Put(int id, [FromBody]Company request)
        {
            var item = context.Companies.FirstOrDefault(c => c.Id == id);

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
                var item = context.Companies.FirstOrDefault(c => c.Id == id);
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
