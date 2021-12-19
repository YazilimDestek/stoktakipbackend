using AutoMapper;
using AutoMapper.Configuration;
using CinigazStokEntity;
using CinigazStokService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CinigazStokService.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public const string ftpServerIP = "";
        public const string ftpUserName = "";
        public const string ftpPassword = "";
        public const string ftpPath = "";

        public readonly StokDbContext context;
        public readonly IMapper mapper;
        public readonly IHostingEnvironment IHostingEnvironment;

        public BaseController(StokDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public BaseController(StokDbContext context, IMapper mapper, IConfiguration configuration, IHostingEnvironment IHostingEnvironment)
        {
            this.context = context;
            this.mapper = mapper;
            this.IHostingEnvironment = IHostingEnvironment;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public Result<string> UploadFile(Byte[] file, string fileName)
        {
            var Return = new Result<string>();
            Return.Meta = new Meta();

            FtpWebRequest objFTPRequest;
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + ftpPath + "/" + fileName));

            // Set Credentials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;
            objFTPRequest.UseBinary = true;
            objFTPRequest.UsePassive = true;
            objFTPRequest.KeepAlive = true;

            try
            {
                byte[] fileContents = file;
                objFTPRequest.ContentLength = file.Length;
                var requestStream = objFTPRequest.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                Return.Meta.IsSuccess = true;
                Return.Entity = fileName;
                return Return;
            }
            catch (Exception ex)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.ErrorMessage = "Dosya yükleme hatası, tekrar deneyiniz.";
                Return.Meta.Error = ex.Message;
            }

            return Return;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetUserID()
        {
            return int.Parse(User.Claims.First(c => c.Type == "UserID").Value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public User GetUser()
        {
            return JsonConvert.DeserializeObject<User>(User.Claims.First(c => c.Type == "User").Value);
        }
    }
}
