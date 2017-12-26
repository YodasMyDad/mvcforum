namespace MvcForum.Core.DomainModel.General
{
    using System.Collections.Generic;
    using Interfaces;

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int total)
        {
            TotalCount = total;
            TotalPages = total / pageSize;
            if (total % pageSize > 0)
            {
                TotalPages++;
            }
            PageSize = pageSize;
            PageIndex = pageIndex;
            //AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            AddRange(source);
        }

        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }

        public bool HasPreviousPage => PageIndex > 0;

        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }
}