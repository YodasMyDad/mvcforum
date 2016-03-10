using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPostEditService
    {
        void Delete(IList<PostEdit> entity);
        void Delete(PostEdit entity);
        PostEdit Add(PostEdit entity);
        PostEdit Get(Guid id);
        IList<PostEdit> GetByPost(Guid postId);
        IList<PostEdit> GetByMember(Guid memberId);
    }
}
