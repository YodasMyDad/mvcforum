using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class MembershipUserPointsService : IMembershipUserPointsService
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

        public void Delete(int amount, MembershipUser user)
        {
            _membershipUserPointsRepository.Delete(amount, user);
        }

        public void Delete(MembershipUser user, PointsFor type, Guid referenceId)
        {
            _membershipUserPointsRepository.Delete(user, type, referenceId);
        }

        public void Delete(PointsFor type, Guid referenceId)
        {
            _membershipUserPointsRepository.Delete(type, referenceId);
        }

        public void Delete(MembershipUser user, PointsFor type)
        {
            _membershipUserPointsRepository.Delete(user, type);
        }

        /// <summary>
        /// Return points by user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="removeTracking">Set this to false if you want to use the results in a database update/insert method</param>
        /// <returns></returns>
        public IEnumerable<MembershipUserPoints> GetByUser(MembershipUser user, bool removeTracking = true)
        {
            return _membershipUserPointsRepository.GetByUser(user, removeTracking);
        }

        /// <summary>
        /// Add new point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public MembershipUserPoints Add(MembershipUserPoints points)
        {
            if (points.Points != 0)
            {
                // Add Date
                points.DateAdded = DateTime.UtcNow;

                // Check this point has not already been awarded
                var canAddPoints = true;

                // Check to see if this has an id
                if (points.PointsForId != null)
                {
                    var alreadyHasThisPoint = GetByUser(points.User).Any(x => x.PointsFor == points.PointsFor && x.PointsForId == points.PointsForId);
                    canAddPoints = (alreadyHasThisPoint == false);
                }

                // If they can ad points let them
                if (canAddPoints)
                {
                    return _membershipUserPointsRepository.Add(points);
                }
            }

            // If not just return the same one back
            return points;
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

        public bool SyncUserPoints(MembershipUser user)
        {
            var needsDbUpdate = false;
            var currentPoints = user.Points.Sum(x => x.Points);
            var dbPoints = _membershipUserPointsRepository.UserPoints(user);
            if (currentPoints != dbPoints)
            {
                // TODO - Update member points here

                needsDbUpdate = true;
            }
            return needsDbUpdate;
        }

        public int PointsByType(MembershipUser user, PointsFor type)
        {
            return GetByUser(user).Where(x => x.PointsFor == type).Sum(x => x.Points);
        }
    }
}
