using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class FavouriteRepository : IFavouriteRepository
    {
        private readonly MVCForumContext _context;
        public FavouriteRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }
        public Favourite Add(Favourite dialogueFavourite)
        {
            return _context.Favourite.Add(dialogueFavourite);
        }

        public Favourite Delete(Favourite dialogueFavourite)
        {
            return _context.Favourite.Remove(dialogueFavourite);
        }

        public List<Favourite> GetAll()
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                .ToList();
        }

        public List<Favourite> GetAllPostFavourites(List<Guid> postIds)
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                            .Where(x => postIds.Contains(x.Post.Id)).ToList();
        }

        public List<Favourite> GetAllByMember(Guid memberId)
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                .Where(x => x.Member.Id == memberId).ToList();
        }

        public Favourite GetByMemberAndPost(Guid memberId, Guid postId)
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                            .FirstOrDefault(x => x.Member.Id == memberId && x.Post.Id == postId);
        }

        public List<Favourite> GetByTopic(Guid topicId)
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                            .Where(x => x.Topic.Id == topicId).ToList();
        }
    }
}
