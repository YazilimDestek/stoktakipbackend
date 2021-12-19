using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CinigazStokService.Models
{
    public class paginationModel<T> where T : class 
    {

        public IQueryable<T> EntityQueryable { get; set; }

        public basePaginationModel BasePaginationModel { get; set; }

    }

    public class basePaginationModel
    {
        public int CurrentPage { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
        public bool IsLastPage { get; set; }
    }

  
}
