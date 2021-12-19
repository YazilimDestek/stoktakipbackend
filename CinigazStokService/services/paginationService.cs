using CinigazStokService.Models;
using System;
using System.Linq;

namespace CinigazStokService.services
{

    public interface IpaginationService
    {
        paginationModel<T> PaginationRequest<T>(int? page, int? pageSize, IQueryable<T> query) where T : class;
    }
    public class paginationService : IpaginationService
    {
        private readonly int defaultPage = 1;
        private readonly int maxPageSize = 500;
        private readonly int defaultPageSize = 50;



        public paginationModel<T> PaginationRequest<T>(int? page, int? pageSize, IQueryable<T> query) where T : class
        {
            var @return = new paginationModel<T>();
            @return.BasePaginationModel=new basePaginationModel();
            var currentPage = tryToGetPage(page);
            var PageSize = tryToGetPageSize(pageSize);
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(decimal.Divide(totalCount, PageSize));
            var isLastPage = currentPage == totalPages;
            var currentPageRequest = currentPage > totalPages ? defaultPage : currentPage; //page sayısını aşan bir page requesti gelirse baştaki pageden başlatmak için

            var records = query.Skip((currentPageRequest - 1) * PageSize).Take(PageSize);

            @return.EntityQueryable = records;
            @return.BasePaginationModel.TotalCount = totalCount;
            @return.BasePaginationModel.TotalPages = totalPages;
            @return.BasePaginationModel.IsLastPage = isLastPage;
            @return.BasePaginationModel.PageSize = PageSize;
            @return.BasePaginationModel.CurrentPage = currentPageRequest;



            return @return;
        }

        private int tryToGetPage(int? page)
        {

            if (page == null || defaultPage <= 0)
            {
                return defaultPage;
            }

            return page.Value;
        }

        private int tryToGetPageSize(int? pageSize)
        {
            if (pageSize == null || pageSize == 0 || pageSize < 0)
            {
                return defaultPageSize;
            }

            if (pageSize > maxPageSize)
            {
                return maxPageSize;
            }

            return pageSize.Value;
        }
    }


}
