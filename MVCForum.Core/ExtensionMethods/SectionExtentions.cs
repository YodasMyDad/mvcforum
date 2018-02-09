namespace MvcForum.Core.ExtensionMethods
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Models.Entities;

    public static class SectionExtentions
    {
        public static IEnumerable<SelectListItem> ToSelectList(this List<Section> sections, bool addEmptyFirstItem = true)
        {
            var selectList = sections.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            if (addEmptyFirstItem)
            {
               selectList.Insert(0, new SelectListItem{ Value = "", Text = ""});
            }

            return selectList;
        }
    }
}