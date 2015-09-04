using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IMembershipUserPointsRepository
    {
        IEnumerable<MembershipUserPoints> GetByUser(MembershipUser user, bool removeTracking = true);
        Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake);
        MembershipUserPoints Add(MembershipUserPoints item);
        MembershipUserPoints Get(Guid id);
        void Update(MembershipUserPoints item);
        int UserPoints(MembershipUser user);
        void Delete(MembershipUser user, PointsFor type, Guid referenceId);
        void Delete(MembershipUser user, PointsFor type);
        void Delete(MembershipUserPoints item);
        void Delete(int amount, MembershipUser user);
        void Delete(IEnumerable<MembershipUserPoints> points);
    }
}

