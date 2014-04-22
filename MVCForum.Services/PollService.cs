using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class PollService : IPollService
    {
        private readonly IPollRepository _pollRepository;

        public PollService(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public List<Poll> GetAllPolls()
        {
            return _pollRepository.GetAllPolls();
        }

        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.UtcNow;
            poll.IsClosed = false;
            return _pollRepository.Add(poll);
        }

        public Poll Get(Guid id)
        {
            return _pollRepository.Get(id);
        }

        public void Delete(Poll item)
        {
            _pollRepository.Delete(item);
        }
    }
}
