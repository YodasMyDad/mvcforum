using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class AkismetViewModel
    {
        [DisplayName("Enable Akisment")]        
        public bool EnableAkisment { get; set; }
        [DisplayName("Akisment Key")]        
        public string AkismentKey { get; set; }
    }

    public class RegistrationQuestionViewModel
    {
        [DisplayName("Your Spam Question")]
        public string SpamQuestion { get; set; }
        [DisplayName("The Answer to the Spam question")]
        public string SpamAnswer { get; set; }
    }

    public class SpamReportingViewModel
    {
        [DisplayName("Enable Spam Reporting")]
        public bool EnableSpamReporting { get; set; }
        [DisplayName("Enable Member Reporting")]
        public bool EnableMemberReporting { get; set; }
    }
}