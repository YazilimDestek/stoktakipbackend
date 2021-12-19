using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CinigazStokService.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {
        public UserController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> Get()
        {
            var items = context.Users.OrderBy(c => c.Username).ToList();
            return items.ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<Result<User>> Get(int id)
        {
            var Return = new Result<User>();
            Return.Meta = new Meta();

            try
            {
                var record = context.Users.FirstOrDefault(c => c.Id == id);
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
        public ActionResult<Result<User>> Post([FromBody]User request)
        {
            var Return = new Result<User>();
            Return.Meta = new Meta();



            var isValidUserName = EnsureUniqueUserName(request.Username);
            if (!isValidUserName.Item1)
            {
                return BadRequest(isValidUserName.Item2);
            }

            var requestedUser = GetUser();
            if (requestedUser.IsAdmin == true)
            {
                try
                {
                    request.Id = 0;
                    request.Password = CreateMD5(request.Password);
                    request.CreatedDateTime = DateTime.Now;
                    request.CreatedUserId = requestedUser.Id;
                    context.Users.Add(request);
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
            else
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "authority.error";
                Return.Meta.ErrorMessage = "Yetkiniz Bulunmamaktadır";
                return Return;

            }

        }

        [HttpPut("{id}")]
        public ActionResult<User> Put(int id, [FromBody]User request)
        {
            var item = context.Users.FirstOrDefault(c => c.Id == id);
            var requestedUser = GetUser();
           
           

            var isValidUserName = EnsureUniqueUserName(request.Username,false);
            if (!isValidUserName.Item1)
            {
                return BadRequest(isValidUserName.Item2);
            }

            if (requestedUser.IsAdmin == true)
            {

                if (!string.IsNullOrEmpty(request.Password))
                {
                    item.Password = CreateMD5(request.Password);
                }

                item.UpdatedDateTime = DateTime.Now;
                item.UpdatedUserId = requestedUser.Id;
                item.Username = request.Username;
                item.Surname = request.Surname;
                item.Email = request.Email;
                item.Name = request.Name;
                item.CategoryAdd = request.CategoryAdd;
                item.CategoryAEdit = request.CategoryAEdit;
                item.CategoryDelete = request.CategoryDelete;
                item.HistoryAdd = request.HistoryAdd;
                item.HistoryEdit = request.HistoryEdit;
                item.HistoryDelete = request.HistoryDelete;
                item.ItemAdd = request.ItemAdd;
                item.ItemEdit = request.ItemEdit;
                item.ItemDelete = request.ItemDelete;
                item.IsAdmin = request.IsAdmin;

                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();

            }

           
            return item;

        }


        [HttpPut("update-profile")]
        public ActionResult<User> Put([FromBody]User request)
        {

            var requestedUser = GetUser();
            var item = context.Users.FirstOrDefault(c => c.Id == requestedUser.Id);

            if (!string.IsNullOrEmpty(request.Password))
            {
                item.Password = CreateMD5(request.Password);
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                item.Email = request.Email;

            }

            item.UpdatedDateTime = DateTime.Now;
            item.UpdatedUserId = requestedUser.Id;
          

            context.Entry(item).State = EntityState.Modified;
            context.SaveChanges();
            return item;

        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(int id)
        {

            var requestedUser = GetUser();
            if (requestedUser.IsAdmin == true)
            {
                try
                {
                    var item = context.Users.FirstOrDefault(c => c.Id == id);
                    item.IsDeleted = true;
                    context.Entry(item).State = EntityState.Modified;
                    context.SaveChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


        private Tuple<bool, string> EnsureUniqueUserName(string username , bool isNewRecord = true)
        {

            if (string.IsNullOrEmpty(username))
            {

                return Tuple.Create(false, "Kullanıcı adı boş bırakılamaz");
            }

            if (isNewRecord)
            {
                var duplicateUser = context.Users.FirstOrDefault(u => u.Username == username && u.IsDeleted == false);

                if (duplicateUser != null)
                {

                    return Tuple.Create(false, "Var olan kullanıcı girdiniz");
                }

            }
            return Tuple.Create(true, "");
        }

    }
}
