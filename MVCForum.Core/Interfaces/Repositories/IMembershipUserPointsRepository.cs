using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IMembershipUserPointsRepository
    {
        IList<MembershipUserPoints> GetByUser(MembershipUser user);
        Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake);
        MembershipUserPoints Add(MembershipUserPoints item);
        MembershipUserPoints Get(Guid id);
        void Delete(MembershipUserPoints item);
        void Delete(int amount, MembershipUser user);
        void Update(MembershipUserPoints item);
    }
}

