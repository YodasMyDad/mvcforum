namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface IFavouriteService : IContextService
    {
        Favourite Add(Favourite dialogueFavourite);
        Favourite Delete(Favourite dialogueFavourite);
        List<Favourite> GetAll();
        Favourite Get(Guid id);
        List<Favourite> GetAllByMember(Guid memberId);
        Favourite GetByMemberAndPost(Guid memberId, Guid postId);
        List<Favourite> GetByTopic(Guid topicId);
        List<Favourite> GetAllPostFavourites(List<Guid> postIds);
    }
}