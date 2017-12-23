namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;

    public partial class MembershipUserPointsService : IMembershipUserPointsService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public MembershipUserPointsService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public void Delete(MembershipUserPoints points)
        {
            _context.MembershipUserPoints.Remove(points);
        }

        public void Delete(int amount, MembershipUser user)
        {
            var points = _context.MembershipUserPoints.Include(x => x.User).FirstOrDefault(x => x.Points == amount && x.User.Id == user.Id);
            Delete(points);
        }

        public void Delete(MembershipUser user, PointsFor type, Guid referenceId)
        {
            var mp = _context.MembershipUserPoints.Include(x => x.User).Where(x => x.User.Id == user.Id && x.PointsFor == type && x.PointsForId == referenceId);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            Delete(mpoints);
        }

        public void Delete(PointsFor type, Guid referenceId)
        {
            var mp = _context.MembershipUserPoints.Where(x => x.PointsFor == type && x.PointsForId == referenceId);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            Delete(mpoints);
        }

        public void Delete(MembershipUser user, PointsFor type)
        {
            var mp = _context.MembershipUserPoints
                            .Include(x => x.User)
                            .Where(x => x.User.Id == user.Id && x.PointsFor == type);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            Delete(mpoints);
        }

        /// <summary>
        /// Return points by user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="removeTracking">Set this to false if you want to use the results in a database update/insert method</param>
        /// <returns></returns>
        public IEnumerable<MembershipUserPoints> GetByUser(MembershipUser user, bool removeTracking = true)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "GetByUser-", user.Id, "-", removeTracking);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var users = _context.MembershipUserPoints.Include(x => x.User).Where(x => x.User.Id == user.Id);
                if (removeTracking)
                {
                    return users.AsNoTracking();
                }
                return users;
            });
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
                    return _context.MembershipUserPoints.Add(points);
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
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "GetCurrentWeeksPoints-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {

                amountToTake = amountToTake ?? int.MaxValue;
                var date = DateTime.UtcNow;
                var start = date.Date.AddDays(-(int)date.DayOfWeek);
                var end = start.AddDays(7);

                var results = _context.MembershipUserPoints
                    .Include(x => x.User)
                    .Where(x => x.DateAdded >= start && x.DateAdded < end)
                    .ToList();

                return results.GroupBy(x => x.User)
                            .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                            .OrderByDescending(x => x.Value)
                            .Take((int)amountToTake)
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }

        /// <summary>
        /// Return an optional amount of points from the current year, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "GetThisYearsPoints-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                amountToTake = amountToTake ?? int.MaxValue;
                var thisYear = DateTime.UtcNow.Year;

                var results = _context.MembershipUserPoints
                    .Include(x => x.User)
                    .Where(x => x.DateAdded.Year == thisYear)
                    .ToList();

                return results.GroupBy(x => x.User)
                            .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                            .OrderByDescending(x => x.Value)
                            .Take((int)amountToTake)
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }

        /// <summary>
        /// Return an optional amount of points, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "GetAllTimePoints-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                amountToTake = amountToTake ?? int.MaxValue;

                var results = _context.MembershipUserPoints
                    .Include(x => x.User)
                    .ToList();

                return results.GroupBy(x => x.User)
                            .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                            .OrderByDescending(x => x.Value)
                            .Take((int)amountToTake)
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }

        public Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "GetAllTimePointsNegative-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                amountToTake = amountToTake ?? int.MaxValue;

                var results = _context.MembershipUserPoints
                            .Include(x => x.User)
                            .ToList();

                return results.GroupBy(x => x.User)
                            .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                            .OrderBy(x => x.Value)
                            .Take((int)amountToTake)
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }

        public bool SyncUserPoints(MembershipUser user)
        {
            var needsDbUpdate = false;
            var currentPoints = user.Points.Sum(x => x.Points);
            var dbPoints = UserPoints(user);
            if (currentPoints != dbPoints)
            {
                // TODO - Update member points here?

                needsDbUpdate = true;
            }
            return needsDbUpdate;
        }

        public int PointsByType(MembershipUser user, PointsFor type)
        {
            return GetByUser(user).Where(x => x.PointsFor == type).Sum(x => x.Points);
        }

        public MembershipUserPoints Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.MembershipUserPoints.FirstOrDefault(x => x.Id == id));
        }

        public int UserPoints(MembershipUser user)
        {
            var cacheKey = string.Concat(CacheKeys.MembershipUserPoints.StartsWith, "UserPoints-", user.Id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.MembershipUserPoints.Include(x => x.User).AsNoTracking().Where(x => x.User.Id == user.Id).Sum(x => x.Points));
        }

        public void Delete(IEnumerable<MembershipUserPoints> points)
        {
            foreach (var membershipUserPoint in points)
            {
                _context.MembershipUserPoints.Remove(membershipUserPoint);
            }
        }
    }
}
