namespace MvcForum.Core.Data.Context
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Activity;
    using Models.Entities;
    using Models.General;

    public partial class MvcForumContext : DbContext, IMvcForumContext
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public MvcForumContext()
        {
            Configuration.LazyLoadingEnabled = true;
        }

        public virtual DbSet<Activity> Activity { get; set; }
        public virtual DbSet<Badge> Badge { get; set; }
        public virtual DbSet<Block> Block { get; set; }
        public virtual DbSet<BadgeTypeTimeLastChecked> BadgeTypeTimeLastChecked { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Section> Section { get; set; }
        public virtual DbSet<CategoryNotification> CategoryNotification { get; set; }
        public virtual DbSet<CategoryPermissionForRole> CategoryPermissionForRole { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<LocaleResourceKey> LocaleResourceKey { get; set; }
        public virtual DbSet<LocaleStringResource> LocaleStringResource { get; set; }
        public virtual DbSet<MembershipRole> MembershipRole { get; set; }
        public virtual DbSet<MembershipUser> MembershipUser { get; set; }
        public virtual DbSet<MembershipUserPoints> MembershipUserPoints { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Poll> Poll { get; set; }
        public virtual DbSet<PollAnswer> PollAnswer { get; set; }
        public virtual DbSet<PollVote> PollVote { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<PrivateMessage> PrivateMessage { get; set; }
        public virtual DbSet<Settings> Setting { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }
        public virtual DbSet<TopicNotification> TopicNotification { get; set; }
        public virtual DbSet<TagNotification> TagNotification { get; set; }
        public virtual DbSet<Vote> Vote { get; set; }
        public virtual DbSet<TopicTag> TopicTag { get; set; }
        public virtual DbSet<BannedEmail> BannedEmail { get; set; }
        public virtual DbSet<BannedWord> BannedWord { get; set; }
        public virtual DbSet<UploadedFile> UploadedFile { get; set; }
        public virtual DbSet<Favourite> Favourite { get; set; }
        public virtual DbSet<GlobalPermissionForRole> GlobalPermissionForRole { get; set; }

        public virtual DbSet<PostEdit> PostEdit { get; set; }


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

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
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

        /// <inheritdoc />
        public void RollBack()
        {
            foreach (var entry in base.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        // Note - problem with deleted entities:
                        // When an entity is deleted its relationships to other entities are severed. 
                        // This includes setting FKs to null for nullable FKs or marking the FKs as conceptually null (don’t ask!) 
                        // if the FK property is not nullable. You’ll need to reset the FK property values to 
                        // the values that they had previously in order to re-form the relationships. 
                        // This may include FK properties in other entities for relationships where the 
                        // deleted entity is the principal of the relationship–e.g. has the PK 
                        // rather than the FK. I know this is a pain–it would be great if it could be made easier in the future, but for now it is what it is.
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // http://stackoverflow.com/questions/7924758/entity-framework-creates-a-plural-table-name-but-the-view-expects-a-singular-ta
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !string.IsNullOrWhiteSpace(type.Namespace))
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