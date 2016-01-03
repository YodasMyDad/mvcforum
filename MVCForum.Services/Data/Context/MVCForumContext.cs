using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces;

namespace MVCForum.Services.Data.Context
{
    public partial class MVCForumContext : DbContext, IMVCForumContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MVCForumContext()   
        {
            Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<Activity> Activity { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<Block> Block { get; set; }
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
        public DbSet<TagNotification> TagNotification { get; set; }
        public DbSet<Vote> Vote { get; set; }
        public DbSet<TopicTag> TopicTag { get; set; }
        public DbSet<BannedEmail> BannedEmail { get; set; }
        public DbSet<BannedWord> BannedWord { get; set; }
        public DbSet<UploadedFile> UploadedFile { get; set; }
        public DbSet<Favourite> Favourite { get; set; }
        public DbSet<GlobalPermissionForRole> GlobalPermissionForRole { get; set; }
        public DbSet<Email> Email { get; set; }
        public DbSet<PostEdit> PostEdit { get; set; }


        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // http://stackoverflow.com/questions/7924758/entity-framework-creates-a-plural-table-name-but-the-view-expects-a-singular-ta
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                                    .Where(type => !string.IsNullOrEmpty(type.Namespace))
                                    .Where(type => type.BaseType != null && type.BaseType.IsGenericType
                                    && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));
            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
            base.OnModelCreating(modelBuilder);  

        }
    }
}
