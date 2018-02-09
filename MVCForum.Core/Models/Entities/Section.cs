namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Utilities;

    /// <summary>
    ///     Section class is just a way to segment the categories in the main home page
    /// </summary>
    public class Section : ExtendedDataEntity, IBaseEntity
    {
        public Section()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <inheritdoc />
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual IList<Category> Categories { get; set; }

    }
}