using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class ImageReturnModel
    {
        public bool IsSucces { get; set; }

        public string DocumentPath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
