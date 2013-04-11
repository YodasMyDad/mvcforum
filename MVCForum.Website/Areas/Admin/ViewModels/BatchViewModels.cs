using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

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

    }
}