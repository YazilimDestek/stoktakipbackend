using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class SearchItemModel
    {
        public string Barcode { get; set; }
        public string SerialNumber { get; set; }
        public string Name { get; set; }
        public int BrandId { get; set; }
    }
}
