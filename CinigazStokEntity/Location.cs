using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class Location : BaseEntity
    {
        public string Name { get; set; }
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        [ForeignKey("LocationType")]
        public int LocationTypeId { get; set;}
        public LocationType LocationType { get; set; }
    }
}
