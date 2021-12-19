using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models.Category
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string name { get; set; }
        public int topCategoryId { get; set; }
        public CinigazStokEntity.Category TopCategory { get; set; }
        public int minStockCount { get; set; }
        public int maxStockCount { get; set; }
        public string description { get; set; }
        public bool isFifo { get; set; }
        public string abysTypeCode { get; set; }
        public string minStockWarningColor { get; set; }
        public string maxStockWarningColor { get; set; }
        public string outOfStockWarningColor { get; set; }
        public List<CategoryFieldModel> mandatoryFields { get; set; }
        public DateTime createdDateTime { get; set; }
        public string createdUser { get; set; }
        public DateTime updatedDateTime { get; set; }
        public string updatedUser { get; set; }
    }

    public class CategoryFieldModel
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string valueType { get; set; }
        public bool isMandatory { get; set; }
        public bool isRequiredOnDefinition { get; set; }
        public bool isRequiredOnMove { get; set; }
    }
}
