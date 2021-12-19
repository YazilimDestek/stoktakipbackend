using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{

    public class StockHistoryCountModel
    {
        public int TotalStockMovementCount { get; set; }

        public int RippingCount { get; set;}
        public int PlacementCount { get; set; }

        public  List<StockHistoryModel> TimeoutItems { get; set; }

        public int timeoutCount { get; set; }

        public List<LastTwentyDays> LastTwentyDays { get; set; }

    }

    public class LastTwentyDays
    {
        public  DateTime Date { get; set; }
        public int Count { get; set;  }
    }
}

