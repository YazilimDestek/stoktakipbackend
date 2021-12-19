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
    [Route("api/itemtype")]
    [ApiController]
    public class ItemTypeController : BaseController
    {
        public ItemTypeController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<Result<ItemType>> Get()
        {
            var Return = new Result<ItemType>();
            Return.Meta = new Meta();

            try
            {
                var items = context.ItemTypes.Where(c => c.IsDeleted == false).OrderBy(c => c.Name).ToList();
                Return.Entities = items.ToList();
                Return.Meta.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Return.Meta.Error = ex.Message;
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu, tekrar deneyiniz";
                Return.Meta.IsSuccess = false;
            }
            
            return Return;
        }

        [HttpGet("{id}")]
        public ActionResult<Result<ItemType>> Get(int id)
        {
            var Return = new Result<ItemType>();
            Return.Meta = new Meta();

            try
            {
                var type = context.ItemTypes.FirstOrDefault(c => c.Id == id);
                Return.Entity = type;
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
        public ActionResult<Result<ItemType>> Post([FromBody] ItemType request)
        {
            var Return = new Result<ItemType>();
            Return.Meta = new Meta();

            try
            {
                request.Id = 0;
                context.ItemTypes.Add(request);
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
        public ActionResult<ItemType> Put(int id, [FromBody] ItemType request)
        {
            var item = context.ItemTypes.FirstOrDefault(c => c.Id == id);

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
                var item = context.ItemTypes.FirstOrDefault(c => c.Id == id);

                if (item == null)
                {
                    return false;
                }
                else
                {
                    if (context.Items.Any(c => c.ItemKindId == item.Id))
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
