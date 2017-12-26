namespace MvcForum.Core.DomainModel.General
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;

    public class PagedFullList<T> : List<T>, IPagedList<T>
    {
        public PagedFullList(IEnumerable<T> source, int pageIndex, int pageSize, int total)
        {
            TotalCount = total;
            TotalPages = total / pageSize;
            if (total % pageSize > 0)
            {
                TotalPages++;
            }
            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
        }

        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }

        public bool HasPreviousPage => PageIndex > 0;

        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }
}