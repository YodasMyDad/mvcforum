using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{

    public partial class FavouriteService : IFavouriteService
    {
        private readonly IFavouriteRepository _favouriteRepository;
        public FavouriteService(IFavouriteRepository favouriteRepository)
        {
            _favouriteRepository = favouriteRepository;
        }

        public Favourite Add(Favourite dialogueFavourite)
        {
            return _favouriteRepository.Add(dialogueFavourite);
        }

        public Favourite Delete(Favourite dialogueFavourite)
        {
            return _favouriteRepository.Delete(dialogueFavourite);
        }

        public List<Favourite> GetAll()
        {
            return _favouriteRepository.GetAll();
        }

        public List<Favourite> GetAllByMember(Guid memberId)
        {
            return _favouriteRepository.GetAllByMember(memberId);
        }

        public Favourite GetByMemberAndPost(Guid memberId, Guid postId)
        {
            return _favouriteRepository.GetByMemberAndPost(memberId, postId);
        }

        public List<Favourite> GetByTopic(Guid topicId)
        {
            return _favouriteRepository.GetByTopic(topicId);
        }

        public List<Favourite> GetAllPostFavourites(List<Guid> postIds)
        {
            return _favouriteRepository.GetAllPostFavourites(postIds);
        }
    }
}
