using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class Timeout : BaseEntity
    {
        [ForeignKey("Location")]
        public int LocationId { get; set; }
        public Location Location { get; set; }
        [ForeignKey("TransType")]
        public int TransTypeId { get; set; }
        public TransType TransType { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int Days { get; set; }
        public string ColorAfterTimeout { get; set; }
        public string ColorAfterTransfer { get; set; }
    }
}
