namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Models.Entities;

    public class CustomCodeViewModels
    {
        [AllowHtml]
        [DisplayName("Custom Header Code")]
        public string CustomHeaderCode { get; set; }

        [AllowHtml]
        [DisplayName("Custom Footer Code")]
        public string CustomFooterCode { get; set; }
    }

    public class EditSettingsViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Forum Name")]
        [Required]
        [StringLength(200)]
        public string ForumName { get; set; }

        [DisplayName("Forum Url")]
        [Required]
        [StringLength(200)]
        public string ForumUrl { get; set; }

        [DisplayName("Close Forum")]
        [Description("Close the forum for maintenance")]
        public bool IsClosed { get; set; }

        [DisplayName("Allow Rss Feeds")]
        [Description("Show the RSS feed icons for the Topics and Categories")]
        public bool EnableRSSFeeds { get; set; }

        [DisplayName("Show Edited By Details On Posts")]
        public bool DisplayEditedBy { get; set; }

        //[DisplayName("Allow File Attachments On Posts")]
        //public bool EnablePostFileAttachments { get; set; }

        [DisplayName("Allow Posts To Be Marked As Solution")]
        public bool EnableMarkAsSolution { get; set; }

        [DisplayName(
            "Timeframe in days to wait before a reminder email is sent to topic creator, for all topics that have not been marked as solution - Set to 0 to disable")]
        public int MarkAsSolutionReminderTimeFrame { get; set; }

        [DisplayName("Enable Spam Reporting")]
        public bool EnableSpamReporting { get; set; }

        [DisplayName("Enable Emoticons (Smilies)")]
        public bool EnableEmoticons { get; set; }

        [DisplayName("Allow Members To Report Other Members")]
        public bool EnableMemberReporting { get; set; }

        [DisplayName("Allow Email Subscriptions")]
        public bool EnableEmailSubscriptions { get; set; }

        [DisplayName(
            "New Members Must Confirm Their Account Via A Link Sent In An Email - Will not work with Twitter accounts!")]
        public bool NewMemberEmailConfirmation { get; set; }

        [DisplayName("Manually Authorise New Members")]
        public bool ManuallyAuthoriseNewMembers { get; set; }

        [DisplayName("Email Admin On New Member Signup")]
        public bool EmailAdminOnNewMemberSignUp { get; set; }

        [DisplayName("Number Of Topics Per Page")]
        public int TopicsPerPage { get; set; }

        [DisplayName("Number Of Posts Per Page")]
        public int PostsPerPage { get; set; }

        [DisplayName("Number Of Activities Per Page")]
        public int ActivitiesPerPage { get; set; }

        [DisplayName("Allow Private Messages")]
        public bool EnablePrivateMessages { get; set; }

        [DisplayName("Private Message Inbox Max Size")]
        public int MaxPrivateMessagesPerMember { get; set; }

        [DisplayName(
            "Private Message Flood Control - Time In Seconds a user must wait before being allowed to message another user")]
        public int PrivateMessageFloodControl { get; set; }

        [DisplayName("Allow Member Signatures")]
        public bool EnableSignatures { get; set; }

        [DisplayName("Enable Members To Create Polls")]
        public bool EnablePolls { get; set; }

        [DisplayName("Allow Points")]
        public bool EnablePoints { get; set; }

        [DisplayName("Amount Of Points Before A User Can Vote")]
        public int PointsAllowedToVoteAmount { get; set; }

        [DisplayName("Amount Of Points For Extended Profile")]
        public int PointsAllowedForExtendedProfile { get; set; }

        [DisplayName("Points Added Per New Post")]
        public int PointsAddedPerPost { get; set; }

        [DisplayName("Points Added For Positive Vote")]
        public int PointsAddedPostiveVote { get; set; }

        [DisplayName("Points Deducted For Negative Vote")]
        public int PointsDeductedNagativeVote { get; set; }

        [DisplayName("Points Added For A Solution")]
        public int PointsAddedForSolution { get; set; }

        [EmailAddress]
        [DisplayName("Admin Email Address")]
        public string AdminEmailAddress { get; set; }

        [DisplayName("Notification Reply Email Address")]
        [AllowHtml] // We have to put this to allow this type of reply address MvcForum <noreply@mvcforum.com>
        public string NotificationReplyEmail { get; set; }

        [DisplayName("SMTP Server")]
        public string SMTP { get; set; }

        [DisplayName("SMTP Server Username")]
        public string SMTPUsername { get; set; }

        [DisplayName("SMTP Server Password")]
        public string SMTPPassword { get; set; }

        [DisplayName("Optional: SMTP Port")]
        public int? SMTPPort { get; set; }

        [DisplayName("SMTP SSL - Enable SSL for sending via gmail etc...")]
        public bool SMTPEnableSSL { get; set; }

        [DisplayName("Current Theme")]
        [Required]
        public string Theme { get; set; }

        public List<string> Themes { get; set; }

        [DisplayName("New Member Starting Role")]
        public Guid? NewMemberStartingRole { get; set; }

        public List<MembershipRole> Roles { get; set; }

        [DisplayName("Default Language")]
        public Guid? DefaultLanguage { get; set; }

        public List<Language> Languages { get; set; }

        [DisplayName("Enable Akismet Spam Control")]
        public bool EnableAkisment { get; set; }

        [DisplayName("Enter Your Akismet Key Here")]
        public string AkismentKey { get; set; }

        [DisplayName("Enter a Spam registration prevention question")]
        public string SpamQuestion { get; set; }

        [DisplayName("Enter the answer to your Spam question")]
        public string SpamAnswer { get; set; }

        [DisplayName("Enable social logins (Facebook etc...)")]
        public bool EnableSocialLogins { get; set; }

        [DisplayName("Disable Standard Registration")]
        public bool DisableStandardRegistration { get; set; }

        [DisplayName("Suspend the registration (Don't allow any new members to register)")]
        public bool SuspendRegistration { get; set; }

        [DisplayName("Page Title")]
        [MaxLength(80)]
        public string PageTitle { get; set; }

        [DisplayName("Meta Desc")]
        [MaxLength(200)]
        public string MetaDesc { get; set; }

        [DisplayName("Disable Dislike Button - Users can only Like posts")]
        public bool DisableDislikeButton { get; set; }

        [DisplayName("New Members must agree to the Terms & Conditions below before using the forum")]
        public bool AgreeToTermsAndConditions { get; set; }

        [DisplayName("Terms & Conditions of the forum")]
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        [StringLength(6000)]
        public string TermsAndConditions { get; set; }
    }
}