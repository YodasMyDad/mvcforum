using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IMembershipRepository
    {
        MembershipUser GetUser(string username);
        IList<MembershipUser> SearchMembers(string username, int amount);
        PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize);
        MembershipUser GetUserBySlug(string slug);
        MembershipUser GetUserByEmail(string slug);
        IList<MembershipUser> GetUserBySlugLike(string slug);
        IList<MembershipUser> GetUsersById(List<Guid> guids);
        IList<MembershipUser> GetAll();
        PagedList<MembershipUser> GetAll(int pageIndex, int pageSize);
        IList<MembershipUser> GetLatestUsers(int amountToTake);
        IList<MembershipUser> GetLowestPointUsers(int amountToTake);
        int MemberCount();

        MembershipUser Add(MembershipUser item);
        MembershipUser Get(Guid id);
        void Delete(MembershipUser item);
        void Update(MembershipUser item);
    }
}
