using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IMembershipUserPointsService
    {
        void Delete(MembershipUserPoints points);
        void Delete(int amount, MembershipUser user);
        IList<MembershipUserPoints> GetByUser(MembershipUser user);
        MembershipUserPoints Add(MembershipUserPoints points);
        Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake);
    }
}
