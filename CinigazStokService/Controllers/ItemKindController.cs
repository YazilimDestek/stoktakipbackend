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
    [Route("api/itemkind")]
    [ApiController]
    public class ItemKindController : BaseController
    {
        public ItemKindController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<Result<ItemKind>> Get()
        {
            var Return = new Result<ItemKind>();
            Return.Meta = new Meta();

            try
            {
                var items = context.ItemKinds.Where(c => c.IsDeleted == false).OrderBy(c => c.Name).ToList();
                Return.Entities = items.ToList();
                Return.Meta.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception";
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
            }
            
            return Return;
        }

        [HttpGet("{id}")]
        public ActionResult<Result<ItemKind>> Get(int id)
        {
            var Return = new Result<ItemKind>();
            Return.Meta = new Meta();

            try
            {
                var brand = context.ItemKinds.FirstOrDefault(c => c.Id == id);
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
        public ActionResult<Result<ItemKind>> Post([FromBody] ItemKind request)
        {
            var Return = new Result<ItemKind>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                context.ItemKinds.Add(request);
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
        public ActionResult<ItemKind> Put(int id, [FromBody]ItemKind request)
        {
            var item = context.ItemKinds.FirstOrDefault(c => c.Id == id);

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
                var item = context.ItemKinds.FirstOrDefault(c => c.Id == id);

                if (item == null)
                {
                    return false;
                }
                else
                {
                    if(context.Items.Any(c => c.ItemKindId == item.Id))
                    {
                        return false;
                    }

                    item.IsDeleted = true;
                    context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    context.SaveChanges();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
