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
    using Models.Enums;

    public partial class VoteService : IVoteService
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private IMvcForumContext _context;

        public VoteService(IMvcForumContext context, IMembershipUserPointsService membershipUserPointsService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _membershipUserPointsService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public Vote Get(Guid id)
        {
            return _context.Vote.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Vote vote)
        {
            // Delete any points associated with this vote
            _membershipUserPointsService.Delete(PointsFor.Vote, vote.Id);

            // Delete the vote
            _context.Vote.Remove(vote);
        }

        public IList<Vote> GetAllVotesByUser(Guid membershipId)
        {
            return _context.Vote.Where(x => x.VotedByMembershipUser.Id == membershipId).ToList();
        }

        public Dictionary<Guid, List<Vote>> GetVotesByPosts(List<Guid> postIds)
        {
            return _context.Vote.AsNoTracking()
                         .Include(x => x.VotedByMembershipUser)
                         .Include(x => x.User)
                         .Include(x => x.Post)
                         .Where(x => postIds.Contains(x.Post.Id))
                         .ToList()
                         .GroupBy(x => x.Post.Id)
                         .ToDictionary(x => x.Key, x => x.ToList());
        }

        public Dictionary<Guid, Dictionary<Guid, List<Vote>>> GetVotesByTopicsGroupedIntoPosts(List<Guid> topicIds)
        {
            var dict = new Dictionary<Guid, Dictionary<Guid, List<Vote>>>();

            var votesGroupedByTopicId = _context.Vote.AsNoTracking()
                .Include(x => x.VotedByMembershipUser)
                .Include(x => x.User)
                .Include(x => x.Post.Topic)
                .Where(x => topicIds.Contains(x.Post.Topic.Id))
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

        public List<Vote> GetVotesByPost(Guid postId)
        {
            return _context.Vote
            .Include(x => x.VotedByMembershipUser)
            .Include(x => x.User)
            .Include(x => x.Post)
            .Where(x => x.Post.Id == postId).ToList();
        }

        /// <summary>
        /// Add a new vote
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        public Vote Add(Vote vote)
        {

            var e = new VoteEventArgs { Vote = vote };
            EventManager.Instance.FireBeforeVoteMade(this, e);

            if (!e.Cancel)
            {
                _context.Vote.Add(vote);

                EventManager.Instance.FireAfterVoteMade(this, new VoteEventArgs { Vote = vote });
            }

            return vote;
        }



    }
}
