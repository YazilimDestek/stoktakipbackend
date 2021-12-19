using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class BaseModel
    {
        public int CreatedUserId { get; set; }
        public string CreatedUsername { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public int? UpdatedUserId { get; set; }
        public string UpdatedUsername { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
