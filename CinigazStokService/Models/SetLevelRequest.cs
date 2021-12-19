using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class SetLevelRequest
    {
        public string SerialNumber { get; set; }
        public int BrandId { get; set; }
        public int? VariantId { get; set; }
        public int LocationId { get; set; }
        public int Level { get; set; }
    }
}
