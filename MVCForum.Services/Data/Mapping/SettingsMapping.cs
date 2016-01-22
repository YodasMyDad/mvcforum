using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class SettingsMapping : EntityTypeConfiguration<Settings>
    {
        public SettingsMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.ForumName).IsOptional().HasMaxLength(500);
            Property(x => x.ForumUrl).IsOptional().HasMaxLength(500);
            Property(x => x.IsClosed).IsOptional();
            Property(x => x.EnableRSSFeeds).IsOptional();
            Property(x => x.DisplayEditedBy).IsOptional();
            Property(x => x.EnablePostFileAttachments).IsOptional();
            Property(x => x.EnableMarkAsSolution).IsOptional();
            Property(x => x.EnableSpamReporting).IsOptional();
            Property(x => x.EnableMemberReporting).IsOptional();
            Property(x => x.EnableEmailSubscriptions).IsOptional();
            Property(x => x.ManuallyAuthoriseNewMembers).IsOptional();
            Property(x => x.EmailAdminOnNewMemberSignUp).IsOptional();
            Property(x => x.TopicsPerPage).IsOptional();
            Property(x => x.PostsPerPage).IsOptional();
            Property(x => x.EnablePrivateMessages).IsOptional();
            Property(x => x.MaxPrivateMessagesPerMember).IsOptional();
            Property(x => x.PrivateMessageFloodControl).IsOptional();
            Property(x => x.EnableSignatures).IsOptional();
            Property(x => x.EnablePoints).IsOptional();
            Property(x => x.PointsAllowedToVoteAmount).IsOptional();
            Property(x => x.PointsAllowedForExtendedProfile).IsOptional();
            Property(x => x.PointsAddedPerPost).IsOptional();
            Property(x => x.PointsAddedPostiveVote).IsOptional();
            Property(x => x.PointsAddedForSolution).IsOptional();
            Property(x => x.PointsDeductedNagativeVote).IsOptional();
            Property(x => x.AdminEmailAddress).IsOptional().HasMaxLength(100);
            Property(x => x.NotificationReplyEmail).IsOptional().HasMaxLength(100);
            Property(x => x.SMTP).IsOptional().HasMaxLength(100);
            Property(x => x.SMTPUsername).IsOptional().HasMaxLength(100);
            Property(x => x.SMTPPort).IsOptional().HasMaxLength(10);
            Property(x => x.SMTPEnableSSL).IsOptional();
            Property(x => x.SMTPPassword).IsOptional().HasMaxLength(100);
            Property(x => x.Theme).IsOptional().HasMaxLength(100);
            Property(x => x.ActivitiesPerPage).IsOptional();
            Property(x => x.EnableAkisment).IsOptional();
            Property(x => x.AkismentKey).IsOptional().HasMaxLength(100);
            Property(x => x.CurrentDatabaseVersion).IsOptional().HasMaxLength(10);
            Property(x => x.SpamQuestion).IsOptional().HasMaxLength(500);
            Property(x => x.SpamAnswer).IsOptional().HasMaxLength(500);
            Property(x => x.EnableSocialLogins).IsOptional();
            Property(x => x.EnablePolls).IsOptional();
            Property(x => x.NewMemberEmailConfirmation).IsOptional();
            Property(x => x.SuspendRegistration).IsOptional();
            Property(x => x.PageTitle).IsOptional().HasMaxLength(80);
            Property(x => x.MetaDesc).IsOptional().HasMaxLength(200);
            Property(x => x.CustomHeaderCode).IsOptional();
            Property(x => x.CustomFooterCode).IsOptional();
            Property(x => x.EnableEmoticons).IsOptional();
            Property(x => x.DisableDislikeButton);
            Property(x => x.AgreeToTermsAndConditions).IsOptional();
            Property(x => x.DisableStandardRegistration).IsOptional();
            Property(x => x.TermsAndConditions).IsOptional();

            HasRequired(t => t.NewMemberStartingRole)
                .WithOptional(x => x.Settings).Map(m => m.MapKey("NewMemberStartingRole"));

            HasRequired(x => x.DefaultLanguage)
                .WithOptional().Map(m => m.MapKey("DefaultLanguage_Id"));
        }
    }
}
