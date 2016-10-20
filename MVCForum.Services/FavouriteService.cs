namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Domain.DomainModel;
    using Domain.Events;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;

    public partial class FavouriteService : IFavouriteService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public FavouriteService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
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
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                                                                    .Include(x => x.Post)
                                                                    .Include(x => x.Topic.Category)
                                                                    .Include(x => x.Member)
                                                                    .ToList());
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
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "GetAllByMember-", memberId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                                                                    .Include(x => x.Post)
                                                                    .Include(x => x.Topic.Category)
                                                                    .Include(x => x.Member)
                                                                    .Where(x => x.Member.Id == memberId).ToList());
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
            var cacheKey = string.Concat(CacheKeys.Favourite.StartsWith, "GetAllPostFavourites-", postIds.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () => _context.Favourite
                                                                        .Include(x => x.Post)
                                                                        .Include(x => x.Topic.Category)
                                                                        .Include(x => x.Member)
                                                                        .Where(x => postIds.Contains(x.Post.Id)).ToList());
        }
    }
}
