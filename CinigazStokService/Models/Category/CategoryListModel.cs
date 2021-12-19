using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models.Category
{
    public class CategoryListModel
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string topCategoryName { get; set; }
        public int currentStock { get; set; }
        public string currentStatus { get; set; }
        public List<CategoryFieldModel> mandatoryFields { get; set; }
    }
}
