namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Constants;
    using EqualityComparers;
    using Interfaces;
    using Interfaces.Pipeline;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Enums;
    using Pipeline;
    using Reflection;

    public partial class MembershipUserPointsService : IMembershipUserPointsService
    {
        private IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        public MembershipUserPointsService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(MembershipUserPoints points)
        {
            // Get the pipelines
            var pointsPipes = ForumConfiguration.Instance.PipelinesPointsDelete;

            // The model to process
            var piplineModel = new PipelineProcess<MembershipUserPoints>(points);

            // Add extended data
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<MembershipUserPoints>, MembershipUserPoints>(_context);

            // Register the pipes 
            var allPointsPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<MembershipUserPoints>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in pointsPipes)
            {
                if (allPointsPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPointsPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(int amount, MembershipUser user)
        {
            var points = _context.MembershipUserPoints.Include(x => x.User).FirstOrDefault(x => x.Points == amount && x.User.Id == user.Id);
            return await Delete(points);
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(MembershipUser user, PointsFor type, Guid referenceId)
        {
            var mp = _context.MembershipUserPoints.Include(x => x.User).Where(x => x.User.Id == user.Id && x.PointsFor == type && x.PointsForId == referenceId);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            return await Delete(mpoints);
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(PointsFor type, Guid referenceId)
        {
            var mp = _context.MembershipUserPoints.Where(x => x.PointsFor == type && x.PointsForId == referenceId);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            return await Delete(mpoints);
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(MembershipUser user, PointsFor type)
        {
            var mp = _context.MembershipUserPoints
                            .Include(x => x.User)
                            .Where(x => x.User.Id == user.Id && x.PointsFor == type);
            var mpoints = new List<MembershipUserPoints>();
            mpoints.AddRange(mp);
            return await Delete(mpoints);
        }

        /// <summary>
        /// Return points by user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="removeTracking">Set this to false if you want to use the results in a database update/insert method</param>
        /// <returns></returns>
        public IEnumerable<MembershipUserPoints> GetByUser(MembershipUser user, bool removeTracking = true)
        {

            var users = _context.MembershipUserPoints.Include(x => x.User).Where(x => x.User.Id == user.Id);
            return removeTracking ? users.AsNoTracking() : users;
        }

        /// <summary>
        /// Add new point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public async Task<IPipelineProcess<MembershipUserPoints>> Add(MembershipUserPoints points)
        {
            // Get the pipelines
            var pointsPipes = ForumConfiguration.Instance.PipelinesPointsCreate;

            // The model to process
            var piplineModel = new PipelineProcess<MembershipUserPoints>(points);

            // Add extended data
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<MembershipUserPoints>, MembershipUserPoints>(_context);

            // Register the pipes 
            var allPointsPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<MembershipUserPoints>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in pointsPipes)
            {
                if (allPointsPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPointsPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        /// <summary>
        /// Return an optional amount of points from the current week, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake)
        {

            var comparer = new EntityEqualityComparer<MembershipUser>();

            amountToTake = amountToTake ?? int.MaxValue;
            var date = DateTime.UtcNow;
            var start = date.Date.AddDays(-(int)date.DayOfWeek);
            var end = start.AddDays(7);

            // We tolist here as GroupBy is expensive operation on the DB with EF
            var points = _context.MembershipUserPoints.AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.DateAdded >= start && x.DateAdded < end).ToList();

            return points.GroupBy(x => x.User, comparer)
                    .Select(x => new
                    {
                        User = x.Key,
                        Points = x.Select(p => p.Points).Sum()
                    })
                    .OrderByDescending(x => x.Points)
                    .Take((int)amountToTake)
                    .ToDictionary(x => x.User, x => x.Points);
        }

        /// <summary>
        /// Return an optional amount of points from the current year, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake)
        {
            var comparer = new EntityEqualityComparer<MembershipUser>();

            amountToTake = amountToTake ?? int.MaxValue;
            var thisYear = DateTime.UtcNow.Year;

            // We tolist here as GroupBy is expensive operation on the DB with EF
            var points = _context.MembershipUserPoints.AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.DateAdded.Year == thisYear)
                .ToList();

            return points.GroupBy(x => x.User, comparer)
                .Select(x => new
                {
                    User = x.Key,
                    Points = x.Select(p => p.Points).Sum()
                })
                .OrderByDescending(x => x.Points)
                .Take((int)amountToTake)
                .ToDictionary(x => x.User, x => x.Points);
        }

        /// <summary>
        /// Return an optional amount of points, returned ordered by amount of points per user
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake)
        {
            var comparer = new EntityEqualityComparer<MembershipUser>();

            amountToTake = amountToTake ?? int.MaxValue;

            var points = _context.MembershipUserPoints.AsNoTracking()
                .Include(x => x.User).ToList();

            return points.GroupBy(x => x.User, comparer)
                .Select(x => new
                {
                    User = x.Key,
                    Points = x.Select(p => p.Points).Sum()
                })
                .OrderByDescending(x => x.Points)
                .Take((int)amountToTake)
                .ToDictionary(x => x.User, x => x.Points);
        }

        public Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake)
        {
            var comparer = new EntityEqualityComparer<MembershipUser>();

            amountToTake = amountToTake ?? int.MaxValue;

            var points = _context.MembershipUserPoints.AsNoTracking()
                .Include(x => x.User).ToList();

            return points.GroupBy(x => x.User, comparer)
                .Select(x => new
                {
                    User = x.Key,
                    Points = x.Select(p => p.Points).Sum()
                })
                .OrderBy(x => x.Points)
                .Take((int)amountToTake)
                .ToDictionary(x => x.User, x => x.Points);
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
            return _context.MembershipUserPoints.FirstOrDefault(x => x.Id == id);
        }

        public int UserPoints(MembershipUser user)
        {
            return _context.MembershipUserPoints.AsNoTracking().Include(x => x.User).AsNoTracking().Where(x => x.User.Id == user.Id).Sum(x => x.Points);
        }

        public async Task<IPipelineProcess<MembershipUserPoints>> Delete(IEnumerable<MembershipUserPoints> points)
        {
            if (points != null)
            {
                IPipelineProcess<MembershipUserPoints> result = null;
                foreach (var membershipUserPoint in points)
                {
                    result = await Delete(membershipUserPoint);
                    if (!result.Successful)
                    {
                        return result;
                    }
                }
                return result;
            }
            return null;
        }
    }
}
