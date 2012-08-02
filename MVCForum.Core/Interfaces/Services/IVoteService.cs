using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IVoteService
    {
        Vote Add(Vote vote);
        void Delete(Vote vote);
    }
}
