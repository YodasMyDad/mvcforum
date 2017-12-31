namespace MvcForum.Core.Events
{
    using DomainModel;
    using Models.Entities;

    public class FavouriteEventArgs : MvcForumEventArgs
    {
        public Favourite Favourite { get; set; }
    }
}