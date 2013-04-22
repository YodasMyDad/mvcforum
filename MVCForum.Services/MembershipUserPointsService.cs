using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public class MembershipUserPointsService : IMembershipUserPointsService
    {
        private readonly IMembershipUserPointsRepository _membershipUserPointsRepository;

        public MembershipUserPointsService(IMembershipUserPointsRepository membershipUserPointsRepository)
        {
            _membershipUserPointsRepository = membershipUserPointsRepository;
        }

        public void Delete(MembershipUserPoints points)
        {
            _membershipUserPointsRepository.Delete(points);
        }
       
        /// <summary>
        /// Return points by user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<MembershipUserPoints> GetByUser(MembershipUser user)
        {
            return _membershipUserPointsRepository.GetByUser(user);
        }

        /// <summary>
        /// Add new point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public MembershipUserPoints Add(MembershipUserPoints points)
        {
            points.DateAdded = DateTime.UtcNow;
            return _membershipUserPointsRepository.Add(points);
        }

        /// <summary>
        /// Return an optional amount of points from the current week, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake)
        {
            return _membershipUserPointsRepository.GetCurrentWeeksPoints(amountToTake);
        }

        /// <summary>
        /// Return an optional amount of points from the current year, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake)
        {
            return _membershipUserPointsRepository.GetThisYearsPoints(amountToTake);
        }

        /// <summary>
        /// Return an optional amount of points, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake)
        {
            return _membershipUserPointsRepository.GetAllTimePoints(amountToTake);
        }

        public Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake)
        {
            return _membershipUserPointsRepository.GetAllTimePointsNegative(amountToTake);
        }
    }
}
