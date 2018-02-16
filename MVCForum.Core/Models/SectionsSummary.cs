namespace MvcForum.Core.Models
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Used when listing summaries
    /// </summary>
    public class SectionSummary
    {
        public Section Section { get; set; }
        public List<CategorySummary> CategorySummaries { get; set; }
    }
}