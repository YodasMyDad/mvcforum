using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class MembershipUserMapping : EntityTypeConfiguration<MembershipUser>
    {
        public MembershipUserMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.UserName).IsRequired().HasMaxLength(150)
                                    .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipUser_UserName", 1) { IsUnique = true }));
            Property(x => x.Password).IsRequired().HasMaxLength(128);
            Property(x => x.PasswordSalt).IsOptional().HasMaxLength(128);
            Property(x => x.Email).IsOptional().HasMaxLength(256);
            Property(x => x.PasswordQuestion).IsOptional().HasMaxLength(256);
            Property(x => x.PasswordAnswer).IsOptional().HasMaxLength(256);
            Property(x => x.IsApproved).IsRequired();
            Property(x => x.IsLockedOut).IsRequired();
            Property(x => x.IsBanned).IsRequired();
            Property(x => x.CreateDate).IsRequired();
            Property(x => x.LastLoginDate).IsRequired();
            Property(x => x.LastPasswordChangedDate).IsRequired();
            Property(x => x.LastLockoutDate).IsRequired();
            Property(x => x.FailedPasswordAttemptCount).IsRequired();
            Property(x => x.FailedPasswordAnswerAttempt).IsRequired();
            Property(x => x.PasswordResetToken).HasMaxLength(150).IsOptional();
            Property(x => x.PasswordResetTokenCreatedAt).IsOptional();
            Property(x => x.Slug).IsRequired().HasMaxLength(150)
                                    .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipUser_Slug", 1) { IsUnique = true }));
            Property(x => x.Comment).IsOptional();
            Property(x => x.Signature).IsOptional().HasMaxLength(1000);
            Property(x => x.Age).IsOptional();
            Property(x => x.Location).IsOptional().HasMaxLength(100);
            Property(x => x.Website).IsOptional().HasMaxLength(100);
            Property(x => x.Twitter).IsOptional().HasMaxLength(60);
            Property(x => x.Facebook).IsOptional().HasMaxLength(60);
            Property(x => x.Avatar).IsOptional().HasMaxLength(500);
            Property(x => x.FacebookAccessToken).IsOptional().HasMaxLength(300);
            Property(x => x.FacebookId).IsOptional();
            Property(x => x.MicrosoftAccessToken).IsOptional().HasMaxLength(450);
            Property(x => x.MicrosoftId).IsOptional();
            Property(x => x.TwitterAccessToken).IsOptional().HasMaxLength(300);
            Property(x => x.TwitterId).IsOptional().HasMaxLength(150);
            Property(x => x.GoogleAccessToken).IsOptional().HasMaxLength(300);
            Property(x => x.GoogleId).IsOptional().HasMaxLength(150);
            Property(x => x.IsExternalAccount).IsOptional();
            Property(x => x.TwitterShowFeed).IsOptional();
            Property(x => x.DisableEmailNotifications).IsOptional();
            Property(x => x.DisablePosting).IsOptional();
            Property(x => x.DisablePrivateMessages).IsOptional();
            Property(x => x.DisableFileUploads).IsOptional();
            Property(x => x.LoginIdExpires).IsOptional();
            Property(x => x.MiscAccessToken).IsOptional().HasMaxLength(250);
            Property(x => x.Latitude).IsOptional().HasMaxLength(40);
            Property(x => x.Longitude).IsOptional().HasMaxLength(40);
            Property(x => x.LastActivityDate).IsOptional();
            Property(x => x.HasAgreedToTermsAndConditions).IsOptional();

            Ignore(x => x.TotalPoints);
            Ignore(x => x.NiceUrl);

            HasMany(x => x.Topics).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.UploadedFiles).WithRequired(x => x.MembershipUser)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            // Has Many, as a user has many posts
            HasMany(x => x.Posts).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.Votes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.VotesGiven).WithOptional(x => x.VotedByMembershipUser)
                .Map(x => x.MapKey("VotedByMembershipUser_Id"));

            HasMany(x => x.TopicNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.Polls).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.PollVotes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.CategoryNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.TagNotifications).WithRequired(x => x.User)
            .Map(x => x.MapKey("MembershipUser_Id"))
            .WillCascadeOnDelete(false);

            HasMany(x => x.Points).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            HasMany(x => x.PrivateMessagesReceived)
                    .WithRequired(x => x.UserTo)
                    .Map(x => x.MapKey("UserTo_Id"))
                    .WillCascadeOnDelete(false);

            HasMany(x => x.PrivateMessagesSent)
                        .WithRequired(x => x.UserFrom)
                        .Map(x => x.MapKey("UserFrom_Id"))
                        .WillCascadeOnDelete(false);

            HasMany(x => x.BadgeTypesTimeLastChecked).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

            // Many-to-many join table - a user may belong to many roles
            HasMany(t => t.Roles)
            .WithMany(t => t.Users)
            .Map(m =>
            {
                m.ToTable("MembershipUsersInRoles");
                m.MapLeftKey("UserIdentifier");
                m.MapRightKey("RoleIdentifier");
            });
           
            // Many-to-many join table - a badge may belong to many users
            HasMany(t => t.Badges)
           .WithMany(t => t.Users)
           .Map(m =>
           {
               m.ToTable("MembershipUser_Badge");
               m.MapLeftKey("MembershipUser_Id");
               m.MapRightKey("Badge_Id");
           });

            HasMany(x => x.PostEdits)
                .WithRequired(x => x.EditedBy)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);

        }
    }
}
