namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class FavouriteEventArgs : MVCForumEventArgs
    {
        public Favourite Favourite { get; set; }
    }
}