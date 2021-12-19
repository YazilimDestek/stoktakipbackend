using CinigazStokService.Models.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class MoveRequestModel
    { 
        public int BrandId { get; set; }
        public string SerialNumber { get; set; }
        public int? VariantId { get; set; }
        public int LocationId { get; set; }
        public int FromLocationId { get; set; }
        public int TransTypeId { get; set; }
        public int Qty { get; set; }
        public List<RequiredFieldValues> RequiredFieldValues { get; set; }
        public string documentPath { get; set; }
    }
}
