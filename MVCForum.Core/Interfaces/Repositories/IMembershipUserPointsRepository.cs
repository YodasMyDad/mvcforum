using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IMembershipUserPointsRepository
    {
        IList<MembershipUserPoints> GetByUser(MembershipUser user);
        Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake);

        MembershipUserPoints Add(MembershipUserPoints item);
        MembershipUserPoints Get(Guid id);
        void Delete(MembershipUserPoints item);
        void Update(MembershipUserPoints item);
    }
}

