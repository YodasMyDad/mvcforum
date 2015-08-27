using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
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

        public Favourite Add(Favourite favourite)
        {
            var e = new FavouriteEventArgs { Favourite = favourite };
            EventManager.Instance.FireBeforeFavourite(this, e);

            if (!e.Cancel)
            {
                favourite = _favouriteRepository.Add(favourite);

                EventManager.Instance.FireAfterFavourite(this, new FavouriteEventArgs { Favourite = favourite});
            }

            return favourite;
        }

        public Favourite Delete(Favourite favourite)
        {
            return _favouriteRepository.Delete(favourite);
        }

        public List<Favourite> GetAll()
        {
            return _favouriteRepository.GetAll();
        }

        public Favourite Get(Guid id)
        {
            return _favouriteRepository.Get(id);
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
