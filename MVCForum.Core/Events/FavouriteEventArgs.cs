namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class FavouriteEventArgs : MvcForumEventArgs
    {
        public Favourite Favourite { get; set; }
    }
}