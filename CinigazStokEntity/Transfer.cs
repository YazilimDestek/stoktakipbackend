using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class Transfer : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public int DestinationLocationId { get; set; }
        public int TransactionTypeId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public List<string> SerialNumbers { get; set; }
    }
}
