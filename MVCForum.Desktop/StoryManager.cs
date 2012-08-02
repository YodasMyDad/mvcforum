using System.Collections.Generic;
using n3oWhiteSite.Domain.DomainModel;
using n3oWhiteSite.Domain.Interfaces.Services;

namespace n3oWhiteSite.Desktop
{
    public class StoryManager
    {
        private readonly IMembershipService _membershipService;


        public StoryManager(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

    }
}
