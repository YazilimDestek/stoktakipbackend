using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class StockLevel : BaseEntity
    {
        [ForeignKey("Location")]
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public string Barcode { get; set; }

        // item
        public string SerialNumber { get; set; }
        public int BrandId { get; set; }

        [ForeignKey("SerialNumber, BrandId")]
        public Item Item { get; set; }

        [ForeignKey("ItemVariant")]
        public int? VariantId { get; set; }
        public ItemVariant ItemVariant { get; set; }
        public int Qty { get; set; }
    }
}
