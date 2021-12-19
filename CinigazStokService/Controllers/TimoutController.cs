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
    [Route("api/timeout")]
    [ApiController]
    public class TimeoutController : BaseController
    {
        public TimeoutController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<Timeout>> Get()
        {
            var items = context.Timeouts.Where(c=>c.IsDeleted == false).Include(c => c.Location).Include(c => c.TransType).Include(c => c.Category).OrderBy(c => c.CreatedDateTime).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<Timeout>> Get(int id)
        {
            var Return = new Result<Timeout>();
            Return.Meta = new Meta();

            try
            {
                var record = context.Timeouts.Include(c=>c.Location).Include(c=>c.TransType).Include(c=>c.Category).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
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
        public ActionResult<Result<Timeout>> Post([FromBody]Timeout request)
        {
            var Return = new Result<Timeout>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                request.CreatedDateTime = DateTime.Now;
                request.CreatedUserId = GetUserID();

                context.Timeouts.Add(request);
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
        public ActionResult<Timeout> Put(int id, [FromBody]Timeout request)
        {
            var item = context.Timeouts.FirstOrDefault(c => c.Id == id);

            item.UpdatedDateTime = DateTime.Now;
            item.UpdatedUserId = GetUserID();
            item.CategoryId = request.CategoryId;
            item.ColorAfterTimeout = request.ColorAfterTimeout;
            item.ColorAfterTransfer = request.ColorAfterTransfer;
            item.Days = request.Days;
            item.LocationId = request.LocationId;
            item.TransTypeId = request.TransTypeId;
            
            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return item;

        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(int id)
        {
            try
            {
                var item = context.Timeouts.FirstOrDefault(c => c.Id == id);
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
