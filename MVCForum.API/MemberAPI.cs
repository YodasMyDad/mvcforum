using System;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.API
{
    public class MemberAPI : IMemberAPI
    {
        private readonly IMembershipRepository _membershipRepository;

        public MemberAPI(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        public MembershipUser GetMemberById(Guid memberId)
        {
            return _membershipRepository.Get(memberId);
        }

        public IList<Badge> GetMembersBadges(Guid memberId)
        {
            return _membershipRepository.Get(memberId).Badges;
        }

        public IList<MembershipUserPoints> GetMembersPoints(Guid memberId)
        {
            return _membershipRepository.Get(memberId).Points;
        }

        public MembershipUser  Create(MembershipUser member)
        {
           return _membershipRepository.Add(member);
        }

        public void Delete(MembershipUser member)
        {
            _membershipRepository.Delete(member);
        }
    }
}
