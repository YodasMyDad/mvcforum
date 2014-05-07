using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public partial interface IVoteAPI
    {
        IEnumerable<Vote> GetAllVotesReceived(Guid memberId);
        IEnumerable<Vote> GetAllVotesGiven(Guid memberId);
    }
}
