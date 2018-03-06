namespace MvcForum.Core.Models
{
    public partial class Email
    {
        public string NameTo { get; set; }
        public string EmailTo { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}