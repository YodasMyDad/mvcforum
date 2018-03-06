namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Constants;
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
        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Section Name")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [DisplayName("Section Description")]
        [DataType(DataType.MultilineText)]
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        public string Description { get; set; }

        [DisplayName("Sort Order")]
        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual IList<Category> Categories { get; set; }

        // Should be using this then using EditorFor
        //[DataType(DataType.MultilineText)]
    }
}