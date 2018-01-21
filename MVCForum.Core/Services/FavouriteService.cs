namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Constants;
    using Events;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class FavouriteService : IFavouriteService
    {
        private readonly ICacheService _cacheService;
        private readonly IMvcForumContext _context;

        public FavouriteService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        public Favourite Add(Favourite favourite)
        {
            var e = new FavouriteEventArgs {Favourite = favourite};
            EventManager.Instance.FireBeforeFavourite(this, e);

            if (!e.Cancel)
            {
                favourite = _context.Favourite.Add(favourite);

                EventManager.Instance.FireAfterFavourite(this, new FavouriteEventArgs {Favourite = favourite});
            }

            return favourite;
        }

        public Favourite Delete(Favourite favourite)
        {
            return _context.Favourite.Remove(favourite);
        }

        public List<Favourite> GetAll()
        {
            return _context.Favourite.AsNoTracking()
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .ToList();
        }

        public Favourite Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                .Include(x => x.Post.User)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .FirstOrDefault(x => x.Id == id));
        }

        public List<Favourite> GetAllByMember(Guid memberId)
        {
            return _context.Favourite.AsNoTracking()
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .Where(x => x.Member.Id == memberId).ToList();
        }

        public Favourite GetByMemberAndPost(Guid memberId, Guid postId)
        {
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "GetByMemberAndPost-", memberId, "-", postId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .FirstOrDefault(x => x.Member.Id == memberId && x.Post.Id == postId));
        }

        public List<Favourite> GetByTopic(Guid topicId)
        {
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "GetByTopic-", topicId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .AsNoTracking()
                .Where(x => x.Topic.Id == topicId).ToList());
        }

        public List<Favourite> GetAllPostFavourites(List<Guid> postIds)
        {
            return _context.Favourite.AsNoTracking()
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .Where(x => postIds.Contains(x.Post.Id)).ToList();
        }
    }
}