using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CinigazStokEntity
{
    public class CategoryField : BaseEntity
    {
        public string Name { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public bool IsMandatory { get; set; }
        public string ValueType { get; set; }
        public bool IsRequiredOnDefinition { get; set; }
        public bool IsRequiredOnMove { get; set; }
        public bool IsFifo { get; set; }
    }
}
