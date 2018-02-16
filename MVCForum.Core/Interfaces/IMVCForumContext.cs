namespace MvcForum.Core.Interfaces
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Models.Activity;
    using Models.Entities;
    using Models.General;

    public partial interface IMvcForumContext : IDisposable
    {
        DbSet<Activity> Activity { get; set; }
        DbSet<Badge> Badge { get; set; }
        DbSet<Block> Block { get; set; }
        DbSet<BadgeTypeTimeLastChecked> BadgeTypeTimeLastChecked { get; set; }
        DbSet<Category> Category { get; set; }
        DbSet<Section> Section { get; set; }
        DbSet<CategoryNotification> CategoryNotification { get; set; }
        DbSet<CategoryPermissionForRole> CategoryPermissionForRole { get; set; }
        DbSet<Language> Language { get; set; }
        DbSet<LocaleResourceKey> LocaleResourceKey { get; set; }
        DbSet<LocaleStringResource> LocaleStringResource { get; set; }
        DbSet<MembershipRole> MembershipRole { get; set; }
        DbSet<MembershipUser> MembershipUser { get; set; }
        DbSet<MembershipUserPoints> MembershipUserPoints { get; set; }
        DbSet<Permission> Permission { get; set; }
        DbSet<Poll> Poll { get; set; }
        DbSet<PollAnswer> PollAnswer { get; set; }
        DbSet<PollVote> PollVote { get; set; }
        DbSet<Post> Post { get; set; }
        DbSet<PrivateMessage> PrivateMessage { get; set; }
        DbSet<Settings> Setting { get; set; }
        DbSet<Topic> Topic { get; set; }
        DbSet<TopicNotification> TopicNotification { get; set; }
        DbSet<TagNotification> TagNotification { get; set; }
        DbSet<Vote> Vote { get; set; }
        DbSet<TopicTag> TopicTag { get; set; }
        DbSet<BannedEmail> BannedEmail { get; set; }
        DbSet<BannedWord> BannedWord { get; set; }
        DbSet<UploadedFile> UploadedFile { get; set; }
        DbSet<Favourite> Favourite { get; set; }
        DbSet<GlobalPermissionForRole> GlobalPermissionForRole { get; set; }
        DbSet<PostEdit> PostEdit { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync();

        void RollBack();
    }
}