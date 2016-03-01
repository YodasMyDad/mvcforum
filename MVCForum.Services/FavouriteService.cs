using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{

    public partial class FavouriteService : IFavouriteService
    {
        private readonly MVCForumContext _context;
        public FavouriteService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Favourite Add(Favourite favourite)
        {
            var e = new FavouriteEventArgs { Favourite = favourite };
            EventManager.Instance.FireBeforeFavourite(this, e);

            if (!e.Cancel)
            {
                favourite =  _context.Favourite.Add(favourite);

                EventManager.Instance.FireAfterFavourite(this, new FavouriteEventArgs { Favourite = favourite});
            }

            return favourite;
        }

        public Favourite Delete(Favourite favourite)
        {
            return _context.Favourite.Remove(favourite);
        }

        public List<Favourite> GetAll()
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                .ToList();
        }

        public Favourite Get(Guid id)
        {
            return _context.Favourite
                            .Include(x => x.Post.User)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                .FirstOrDefault(x => x.Id == id);
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
                            .AsNoTracking()
                            .Where(x => x.Topic.Id == topicId).ToList();
        }

        public List<Favourite> GetAllPostFavourites(List<Guid> postIds)
        {
            return _context.Favourite
                            .Include(x => x.Post)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Member)
                            .Where(x => postIds.Contains(x.Post.Id)).ToList();
        }
    }
}
