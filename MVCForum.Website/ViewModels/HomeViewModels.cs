namespace MvcForum.Web.ViewModels
{
    using Application;
    using Application.ActionFilterAttributes;
    using Core.DomainModel.Activity;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class ListCategoriesViewModels
    {
        public MembershipUser MembershipUser { get; set; }
        public MembershipRole MembershipRole { get; set; }
    }

    public class AllRecentActivitiesViewModel
    {
        public PagedList<ActivityBase> Activities { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }

    public class TermsAndConditionsViewModel
    {
        public string TermsAndConditions { get; set; }

        [ForumMvcResourceDisplayName("TermsAndConditions.Label.Agree")]
        [MustBeTrue(ErrorMessage = "TermsAndConditions.Label.AgreeError")]
        public bool Agree { get; set; }
    }
}