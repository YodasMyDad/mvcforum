using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class FavouriteMapping : EntityTypeConfiguration<Favourite>
    {
        public FavouriteMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.DateCreated).IsRequired();
            HasRequired(x => x.Post).WithMany(x => x.Favourites).Map(x => x.MapKey("PostId")).WillCascadeOnDelete(false);
            HasRequired(x => x.Member).WithMany(x => x.Favourites).Map(x => x.MapKey("MemberId")).WillCascadeOnDelete(false);
            HasRequired(x => x.Topic).WithMany(x => x.Favourites).Map(x => x.MapKey("TopicId")).WillCascadeOnDelete(false);
        }
    }
}
