using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class MembershipUserMapping : EntityTypeConfiguration<MembershipUser>
    {
        public MembershipUserMapping()
        {
            HasKey(x => x.Id);

            HasMany(x => x.Topics).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            // Has Many, as a user has many posts
            HasMany(x => x.Posts).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Votes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.TopicNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Polls).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.PollVotes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.CategoryNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Points).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.PrivateMessagesReceived).WithRequired(x => x.UserFrom)
                .WillCascadeOnDelete();

            HasMany(x => x.PrivateMessagesSent)
                .WithRequired(x => x.UserTo)
                .WillCascadeOnDelete();

            HasMany(x => x.BadgeTypesTimeLastChecked).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

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
        }
    }
}
