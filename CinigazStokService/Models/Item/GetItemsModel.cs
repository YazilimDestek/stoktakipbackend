using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class GetItemsModel
    {
        public int? BrandId { get; set; }
        public string SerialNumber { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public int? PageCount { get; set; }
        public int? PageIndex { get; set; }
        public int? StatusId { get; set; }
        public int? ItemKindId { get; set; }
        public int? ItemTypeId { get; set; }
    }
}
