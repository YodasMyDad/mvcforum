using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using MVCForum.Data.Mapping;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.Interfaces;

namespace MVCForum.Data.Context
{
    public class MVCForumContext : DbContext, IMVCForumContext
    {
        // http://blogs.msdn.com/b/adonet/archive/2010/12/06/ef-feature-ctp5-fluent-api-samples.aspx
        public DbSet<Activity> Activity { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<BadgeTypeTimeLastChecked> BadgeTypeTimeLastChecked { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<CategoryNotification> CategoryNotification { get; set; }
        public DbSet<CategoryPermissionForRole> CategoryPermissionForRole { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<LocaleResourceKey> LocaleResourceKey { get; set; }
        public DbSet<LocaleStringResource> LocaleStringResource { get; set; }
        public DbSet<MembershipRole> MembershipRole { get; set; }
        public DbSet<MembershipUser> MembershipUser { get; set; }
        public DbSet<MembershipUserPoints> MembershipUserPoints { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<Poll> Poll { get; set; }
        public DbSet<PollAnswer> PollAnswer { get; set; }
        public DbSet<PollVote> PollVote { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<PrivateMessage> PrivateMessage { get; set; }
        public DbSet<Settings> Setting { get; set; }
        public DbSet<Topic> Topic { get; set; } 
        public DbSet<TopicNotification> TopicNotification { get; set; }
        public DbSet<Vote> Vote { get; set; }
        public DbSet<TopicTag> TopicTag { get; set; }
        public DbSet<BannedEmail> BannedEmail { get; set; }
        public DbSet<BannedWord> BannedWord { get; set; }
        public DbSet<UploadedFile> UploadedFile { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MVCForumContext()   
        {
            Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // http://stackoverflow.com/questions/7924758/entity-framework-creates-a-plural-table-name-but-the-view-expects-a-singular-ta
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Mappings
            modelBuilder.Configurations.Add(new BadgeMapping());
            modelBuilder.Configurations.Add(new BadgeTypeTimeLastCheckedMapping());
            modelBuilder.Configurations.Add(new CategoryMapping());
            modelBuilder.Configurations.Add(new CategoryNotificationMapping());
            modelBuilder.Configurations.Add(new CategoryPermissionForRoleMapping());
            modelBuilder.Configurations.Add(new LanguageMapping());
            modelBuilder.Configurations.Add(new LocaleResourceKeyMapping());
            modelBuilder.Configurations.Add(new LocaleStringResourceMapping());
            modelBuilder.Configurations.Add(new MembershipRoleMapping());
            modelBuilder.Configurations.Add(new MembershipUserMapping());
            modelBuilder.Configurations.Add(new MembershipUserPointsMapping());
            modelBuilder.Configurations.Add(new PermissionMapping());
            modelBuilder.Configurations.Add(new PollAnswerMapping());
            modelBuilder.Configurations.Add(new PollMapping());
            modelBuilder.Configurations.Add(new PollVoteMapping());
            modelBuilder.Configurations.Add(new PostMapping());         
            modelBuilder.Configurations.Add(new PrivateMessageMapping());         
            modelBuilder.Configurations.Add(new SettingsMapping());         
            modelBuilder.Configurations.Add(new TopicMapping());         
            modelBuilder.Configurations.Add(new TopicNotificationMapping());         
            modelBuilder.Configurations.Add(new TopicTagMapping());
            modelBuilder.Configurations.Add(new VoteMapping());
            modelBuilder.Configurations.Add(new BannedEmailMapping());
            modelBuilder.Configurations.Add(new BannedWordMapping());
            modelBuilder.Configurations.Add(new UploadedFileMapping());

            // Ignore properties on domain models
            //modelBuilder.Entity<Category>().Ignore(cat => cat.SubCategories);

            base.OnModelCreating(modelBuilder);
        }

        public new void Dispose()
        {

        }
    }
}
