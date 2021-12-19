using CinigazStokService.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinigazStokService.Models;

namespace CinigazStokService.Handler
{
    public interface IImageHandler
    {
        Task<ImageReturnModel> UploadImage(IFormFile file);
    }

    public class ImageHandler: IImageHandler
    {
        private readonly IImageWriter _imageWriter;
        public ImageHandler(IImageWriter imageWriter)
        {
            _imageWriter = imageWriter;
        }

        public async Task<ImageReturnModel> UploadImage(IFormFile file)
        {
            return await _imageWriter.UploadImage(file);
           
        }
    }
}
