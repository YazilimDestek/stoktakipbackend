using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class Item
    {
        [Key, Column(Order = 0)]
        public string SerialNumber { get; set; }
        [ForeignKey("Brand")]
        [Key, Column(Order = 1)]
        public int BrandId { get; set; } // Markası
        public Brand Brand { get; set; }


        public DateTime CreatedDateTime { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public int? UpdatedUserId { get; set; }
        public string Name { get; set; } // Ürün Adı
        public string Description { get; set; } // Ürün Açıklaması

        // sayaç türü
        [ForeignKey("ItemKind")]
        public int? ItemKindId { get; set; }
        public ItemKind ItemKind { get; set; }

        // sayaç tipi
        [ForeignKey("ItemType")]
        public int? ItemTypeId { get; set; }
        public ItemType ItemType { get; set; }


        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        public string Barcode { get; set; }
        public bool? IsUsingVariants { get; set; }
        public bool IsSingular { get; set; }

        public string Specifications { get; set; }
        public string RequiredFieldValues { get; set; }
    }
}
