namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class FavouriteEventArgs : MvcForumEventArgs
    {
        public Favourite Favourite { get; set; }
    }
}