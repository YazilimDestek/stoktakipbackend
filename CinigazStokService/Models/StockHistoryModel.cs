using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CinigazStokService.Models.Item;

namespace CinigazStokService.Models
{
    public class StockHistoryModel
    {

        public long Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string CreatedUserName { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public int? UpdatedUserId { get; set; }
        public string UpdatedUserName { get; set; }

        // item
        public int BrandId { get; set; }
        public BrandModel Brand { get; set; }
        public string SerialNumber { get; set; }
        public ItemModel Item { get; set; }

        // fields
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public int FromLocationId { get; set; }
        public string FromLocation { get; set; }
        public int ToLocationId { get; set; }
        public string ToLocation { get; set; }
        public int TransTypeId { get; set; }
        public string TransType { get; set; }
        public List<RequiredField> RequiredFields { get; set; } // json -> { "endeks" : 12325 }
        public string categoryName { get; set; }
        public int? VariantId { get; set; }
        public List<VariantParams>  VariantParams { get; set; }
        public string Barcode { get; set; }
        public string DocumentPath { get; set; }
        public List<RequiredFieldValues> ItemRequiredFields { get; set; }

        // timeout'tan gelen history kayıtları
        [NotMapped]
        public string ColorAfterTimeout { get; set; }
        [NotMapped]
        public string ColorAfterTransfer { get; set; }
    }

    public class RequiredField
    {
        public string name { get; set; }
        public string value { get; set; }
        public string valueType { get; set; }
    }

    public class StockHistoryFilter
    {
        public string barcode { get; set; }
        public int fromLocationId { get; set; }

        public int toLocationId { get; set; }
        public int transTypeId { get; set; }
        public DateTime stockStartDate { get; set; }
        public DateTime stockEndDate { get; set; }

        public int? page { get; set; }
        public int? pageSize { get; set; }

    }
}
