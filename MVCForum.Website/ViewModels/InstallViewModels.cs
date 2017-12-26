namespace MvcForum.Web.ViewModels
{
    public class CreateDbViewModel
    {
        public bool IsUpgrade { get; set; }
        public string PreviousVersion { get; set; }
        public string CurrentVersion { get; set; }
    }
}