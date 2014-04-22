using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public partial interface IMemberAPI
    {
        /// <summary>
        /// Create a new member
        /// </summary>
        /// <param name="member"></param>
        MembershipUser Create(MembershipUser member);

        /// <summary>
        /// Delete a member
        /// </summary>
        /// <param name="member"></param>
        void Delete(MembershipUser member);

        /// <summary>
        /// Get all the badges belonging to a member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<Badge> GetMembersBadges(Guid memberId);

        /// <summary>
        /// Get member by id
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns>The member, or null if not found</returns>
        MembershipUser GetMemberById(Guid memberId);

        /// <summary>
        /// Get all a members points
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<MembershipUserPoints> GetMembersPoints(Guid memberId);
    }
}
