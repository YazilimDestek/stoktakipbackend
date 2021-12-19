using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class ItemVariant : BaseEntity
    {
        public string SerialNumber { get; set; }
        public int BrandId { get; set; }
        [ForeignKey("SerialNumber, BrandId")]
        public Item Item { get; set; }
        public string Barcode { get; set; }
        public string VariantParams { get; set; }
    }
}
