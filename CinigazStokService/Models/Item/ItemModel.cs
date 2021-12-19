using CinigazStokEntity;
using CinigazStokService.Models.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models.Item
{
    public class ItemModel : BaseModel
    {
        public bool isSingular { get; set; }
        public bool usingVariants { get; set; }
        public string barcode { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string serialNumber { get; set; }
        public int brandId { get; set; }
        public Brand Brand { get; set; }
        public int categoryId { get; set; }
        public CategoryModel Category { get; set; }
        public List<Variant> variants { get; set; }
        public List<StockHistory> historyRecords { get; set; }
        public List<Specification> specifications { get; set; }
        public List<RequiredFieldValues> requiredFieldValues { get; set; }
        public int? ItemKindId { get; set; }
        public ItemKind ItemKind { get; set; }
        public int? ItemTypeId { get; set; }
        public ItemType ItemType { get; set; }
    }

    public class RequiredFieldValues
    {
        public string name { get; set; }
        public string valueType { get; set; }
        public string value { get; set; }
    }

    public class Variant
    {
        public int Id { get; set; }
        public string barcode { get; set; }
        public List<VariantParams> variantParams { get; set; }
    }

    public class VariantParams
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class Specification
    {
        public string text { get; set; }
        public string value { get; set; }
    }
}
