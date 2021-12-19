using System;
using System.Collections.Generic;
using System.Text;

namespace CinigazStokEntity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public int TopCategoryId { get; set; }
        public string Description { get; set; }
        public string AbysTypeCode { get; set; }
        public int MinStockCount { get; set; }
        public int MaxStockCount { get; set; }
        public string MinStockWarningColor { get; set; }
        public string MaxStockWarningColor { get; set; }
        public string OutOfStockWarningColor { get; set; }
    }
}
