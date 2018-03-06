namespace MvcForum.Web.ViewModels.Admin
{
    using Core.Models.Entities;

    public class SectionAddEditViewModel : Section
    {
        public bool IsEdit { get; set; }
    }
}