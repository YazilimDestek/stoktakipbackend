using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class StockHistory : BaseEntity
    {
        // fields
        public int Qty { get; set; }
        [ForeignKey("FromLocation")]
        public int FromLocationId { get; set; }
        public Location FromLocation { get; set; }
        [ForeignKey("ToLocation")]
        public int ToLocationId { get; set; }
        public Location ToLocation { get; set; }
        [ForeignKey("TransType")]
        public int TransTypeId { get; set; }
        public TransType TransType { get; set; }
        public string RequiredFields { get; set; } // json -> { "endeks" : 12325 }

        // item
        public string SerialNumber { get; set; }
        public int BrandId { get; set; }

        [ForeignKey("SerialNumber, BrandId")]
        public Item Item { get; set; }


        [ForeignKey("Variant")]
        public int? VariantId { get; set; }
        public ItemVariant Variant { get; set; }
        public string Barcode { get; set; }
        public string DocumentPath { get; set; }
    }
}
