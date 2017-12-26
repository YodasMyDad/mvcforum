namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using DomainModel;
    using DomainModel.Entities;

    public class PollAnswerMapping : EntityTypeConfiguration<PollAnswer>
    {
        public PollAnswerMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Answer).IsRequired().HasMaxLength(600);
            HasMany(x => x.PollVotes)
                .WithRequired(t => t.PollAnswer)
                .Map(x => x.MapKey("PollAnswer_Id"))
                .WillCascadeOnDelete(false);
        }
    }
}
