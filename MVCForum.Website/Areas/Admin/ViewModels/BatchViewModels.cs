using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class BatchDeleteMembersViewModel
    {
        [Required]
        [Numeric]
        [DisplayName("Registered in the last specified amount of days")]
        public int AmoutOfDaysSinceRegistered { get; set; }

        [Required]
        [Numeric]
        [DisplayName("And has posted less than or equal to this amount of times")]
        public int AmoutOfPosts { get; set; }

        public int AmountDeleted { get; set; }
    }

    public class BatchMoveTopicsViewModel
    {
        public List<Category> Categories { get; set; }

        [Required]        
        [DisplayName("Move all Topics in this Category")]
        public Guid? FromCategory { get; set; }

        [Required]
        [DisplayName("To this new Category")]
        public Guid? ToCategory { get; set; }
    }
}