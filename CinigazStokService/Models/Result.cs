using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{    public class Result<TEntity> where TEntity : class
    {
        public List<TEntity> Entities { get; set; }
        public TEntity Entity { get; set; }
        public Meta Meta { get; set; }
    }

    public class Meta
    {

        public basePaginationModel BasePaginationModel { get; set; }
        public decimal Summary { get; set; }
        public bool IsSuccess { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<long> SuccessList { get; set; }
        public List<long> FailList { get; set; }
        public string Error { get; set; }
        public string ErrorMessage { get; set; }



    }
}
