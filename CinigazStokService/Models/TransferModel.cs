using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class TransferModel
    {
        public int DestinationLocationId { get; set; }
        public int TransactionTypeId { get; set; }
        public dynamic SerialNumbers { get; set; }
    }
}
