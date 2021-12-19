using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CinigazStokService.Models;

namespace CinigazStokService.Helper
{
    public interface IImageWriter
    {
        Task<ImageReturnModel> UploadImage(IFormFile file);
    }

    public class ImageWriter : IImageWriter
    {
        public async Task<ImageReturnModel> UploadImage(IFormFile file)
        {
            var @return = new ImageReturnModel();


            if (CheckIfImageFile(file))
            {
                var fileName = await WriteFile(file);

                @return.IsSucces = true;
                @return.DocumentPath = fileName;

            }
            else
            {
                @return.IsSucces = false;
                @return.ErrorMessage = "Invalid image file";
            }


            return @return;

        }

        private bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return WriterHelper.GetImageFormat(fileBytes) != WriterHelper.ImageFormat.unknown;
        }
        public async Task<string> WriteFile(IFormFile file)
        {
            string fileName;
            try
            {

                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + extension; //Create a new Name for the file due to security reasons.
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Files\\images", fileName);

                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }
    }
}

