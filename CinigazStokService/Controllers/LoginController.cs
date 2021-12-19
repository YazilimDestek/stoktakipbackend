using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using CinigazStokService.Security;
using CinigazStokService.Security.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CinigazStokService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseController
    {
        public LoginController(StokDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        // GET api/login
        [HttpPost]
        [AllowAnonymous]
        public Result<JwtToken> Login([FromBody]LoginRequest request)
        {
            var Return = new Result<JwtToken>();
            Return.Meta = new Meta();

            try
            {
                if (!ModelState.IsValid)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "LoginRequest modelstate invalid";
                    Return.Meta.ErrorMessage = "Geçersiz form!";
                    return Return;
                }
                 
                var User = context.Users.FirstOrDefault(c => c.Username == request.Username && c.Password == CreateMD5(request.Password));
                if (User == null)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "Username or password is invalid";
                    Return.Meta.ErrorMessage = "Kullanıcı adı veya şifre hatalı";
                    return Return;
                }
                else if (User.IsDeleted)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "Your account was deleted";
                    Return.Meta.ErrorMessage = "Hesabınız Silinmiştir";
                    return Return;
                }
                else
                {
                    var token = new JwtTokenBuilder()
                    .AddSecurityKey(JwtSecurityKey.Create("CINIGAZ2019-92223K-324957-K3596U"))
                    .AddIssuer("entray.com")
                    .AddAudience("entray.com")
                    .AddClaim("User", JsonConvert.SerializeObject(User))
                    .AddClaim("UserID", User.Id.ToString())
                    .AddSubject("entray.com");

                    if (request.Remember == false)
                    {
                        token.AddExpiry(540); // 9 SAAT
                    }
                    else
                    {
                        token.AddExpiry(2000000); // 1388 gün
                    }

                    var tokenWithUserInfo = token.Build();
                    tokenWithUserInfo.User = User;

                    Return.Entity = tokenWithUserInfo;
                    Return.Meta.IsSuccess = true;
                    return Return;
                }
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception : " + ex.Message;
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu.";
                return Return;
            }
        }

        // GET api/login
        [HttpPost("register")]
        [AllowAnonymous]
        public Result<JwtToken> Register([FromBody] LoginRequest request)
        {
            var Return = new Result<JwtToken>();
            Return.Meta = new Meta();

            try
            {
                if (!ModelState.IsValid)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "LoginRequest modelstate invalid";
                    Return.Meta.ErrorMessage = "Geçersiz form!";
                    return Return;
                }

                var User = context.Users.FirstOrDefault(c => c.Username == request.Username && c.Password == CreateMD5(request.Password));
                if (User == null)
                {
                    User = new User();
                    User.CreatedDateTime = DateTime.Now;
                    User.Email = "mobileapp@hesap.co";
                    User.IsActive = true;
                    User.IsAdmin = false;
                    User.Name = request.Username;
                    User.Username = request.Username;
                    User.Password = CreateMD5(request.Password);
                    context.Users.Add(User);
                    context.SaveChanges();
                }
                else if (User.IsDeleted)
                {
                    Return.Meta.IsSuccess = false;
                    Return.Meta.Error = "Your account was deleted";
                    Return.Meta.ErrorMessage = "Hesabınız Silinmiştir";
                    return Return;
                }

                var token = new JwtTokenBuilder()
                .AddSecurityKey(JwtSecurityKey.Create("CINIGAZ2019-92223K-324957-K3596U"))
                .AddIssuer("entray.com")
                .AddAudience("entray.com")
                .AddClaim("User", JsonConvert.SerializeObject(User))
                .AddClaim("UserID", User.Id.ToString())
                .AddSubject("entray.com");

                token.AddExpiry(2000000); // 1388 gün

                var tokenWithUserInfo = token.Build();
                tokenWithUserInfo.User = User;

                Return.Entity = tokenWithUserInfo;
                Return.Meta.IsSuccess = true;
                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "unexpected.exception : " + ex.Message;
                Return.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu.";
                return Return;
            }
        }

    }
}
