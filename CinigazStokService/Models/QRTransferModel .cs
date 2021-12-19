using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class QRTransferModel
    {
        public int DestinationLocationId { get; set; }
        public int TransactionTypeId { get; set; }
        public dynamic QRModel { get; set;}
    }
}