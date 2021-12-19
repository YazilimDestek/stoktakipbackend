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
    [Route("api/transtype")]
    [ApiController]
    public class TransTypeController : BaseController
    {
        public TransTypeController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<TransType>> Get()
        {
            var items = context.TransTypes.Where(c=>c.IsDeleted == false).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }


        [HttpGet("mobilebarcode")]
        public ActionResult<IEnumerable<TransType>> GetMobileBarcodeTransTypes()
        {
            var items = context.TransTypes.Where(c => c.IsDeleted == false && c.UseForMobileBarcode == true).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }

        [HttpGet("mobileqrcode")]
        public ActionResult<IEnumerable<TransType>> GetMobileQrCodeTransTypes()
        {
            var items = context.TransTypes.Where(c => c.IsDeleted == false && c.UseForMobileQrcode == true).OrderBy(c => c.Name).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<TransType>> Get(int id)
        {
            var Return = new Result<TransType>();
            Return.Meta = new Meta();

            try
            {
                var record = context.TransTypes.FirstOrDefault(c => c.Id == id);
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
        public ActionResult<Result<TransType>> Post([FromBody]TransType request)
        {
            var Return = new Result<TransType>();
            Return.Meta = new Meta();

            try
            {
                var transType = new TransType();
                transType.Name = request.Name;
                transType.RefCode = request.RefCode;
                transType.CreatedDateTime = DateTime.Now;
                transType.CreatedUserId = GetUserID();
                transType.UseForMobileBarcode = request.UseForMobileBarcode;
                transType.UseForMobileQrcode = request.UseForMobileQrcode;

                context.TransTypes.Add(transType);
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
        public ActionResult<TransType> Put(int id, [FromBody]TransType request)
        {
            var item = context.TransTypes.FirstOrDefault(c => c.Id == id);

            item.UpdatedDateTime = DateTime.Now;
            item.UpdatedUserId = GetUserID();
            item.Name = request.Name;
            item.RefCode = request.RefCode;
            item.DocumentRequired = request.DocumentRequired;
            item.UseForMobileBarcode = request.UseForMobileBarcode;
            item.UseForMobileQrcode = request.UseForMobileQrcode;
            
            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return item;

        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(int id)
        {
            try
            {
                var item = context.TransTypes.FirstOrDefault(c => c.Id == id);
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
