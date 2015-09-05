using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.Interfaces;

namespace MVCForum.Domain.DomainModel
{
    public partial class PagedFullList<T> : List<T>, IPagedList<T>
    {
        public PagedFullList(IEnumerable<T> source, int pageIndex, int pageSize, int total)
        {
            TotalCount = total;
            TotalPages = total / pageSize;
            if (total % pageSize > 0)
                TotalPages++;
            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
        }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }
        public bool HasNextPage
        {
            get { return (PageIndex + 1 < TotalPages); }
        }
    }
}
