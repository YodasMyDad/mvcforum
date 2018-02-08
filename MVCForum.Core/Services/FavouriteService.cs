namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Events;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class FavouriteService : IFavouriteService
    {
        private IMvcForumContext _context;

        public FavouriteService(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
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

        /// <inheritdoc />
        public Dictionary<Guid, List<Favourite>> GetByTopicGroupedByPost(Guid topicId)
        {
            return _context.Favourite
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .AsNoTracking()
                .Where(x => x.Topic.Id == topicId)
                .ToList()
                .GroupBy(x => x.Post.Id)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        /// <inheritdoc />
        public Dictionary<Guid, Dictionary<Guid, List<Favourite>>> GetByTopicsGroupedIntoPosts(List<Guid> topicIds)
        {
            var dict = new Dictionary<Guid, Dictionary<Guid, List<Favourite>>>();

            var votesGroupedByTopicId = _context.Favourite.AsNoTracking()
                .Include(x => x.Topic)
                .Include(x => x.Post)
                .Include(x => x.Member)
                .Where(x => topicIds.Contains(x.Topic.Id))
                .ToList()
                .ToLookup(x => x.Post.Id);

            foreach (var vgbtid in votesGroupedByTopicId)
            {
                var votesGroupedByPostId = vgbtid
                    .GroupBy(x => x.Post.Id)
                    .ToDictionary(x => x.Key, x => x.ToList());

                dict.Add(vgbtid.Key, votesGroupedByPostId);
            }

            return dict;
        }

        public Dictionary<Guid, List<Favourite>> GetAllPostFavourites(List<Guid> postIds)
        {
            return _context.Favourite.AsNoTracking()
                .Include(x => x.Post)
                .Include(x => x.Topic.Category)
                .Include(x => x.Member)
                .Where(x => postIds.Contains(x.Post.Id))
                .ToList()
                .GroupBy(x => x.Post.Id)
                .ToDictionary(x => x.Key, x => x.ToList()); ;
        }
    }
}