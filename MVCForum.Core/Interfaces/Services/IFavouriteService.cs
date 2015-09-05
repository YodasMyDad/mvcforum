using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IFavouriteService
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
