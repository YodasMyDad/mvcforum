using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class FavouriteEventArgs : MVCForumEventArgs
    {
        public Favourite Favourite { get; set; }
    }
}
