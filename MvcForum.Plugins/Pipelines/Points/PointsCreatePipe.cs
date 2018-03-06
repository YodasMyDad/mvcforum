namespace MvcForum.Plugins.Pipelines.Points
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class PointsCreatePipe : IPipe<IPipelineProcess<MembershipUserPoints>>
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ILoggingService _loggingService;

        public PointsCreatePipe(ILoggingService loggingService, IMembershipUserPointsService membershipUserPointsService)
        {
            _loggingService = loggingService;
            _membershipUserPointsService = membershipUserPointsService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUserPoints>> Process(IPipelineProcess<MembershipUserPoints> input,
            IMvcForumContext context)
        {

            try
            {
                if (input.EntityToProcess.Points != 0)
                {
                    // Add Date
                    input.EntityToProcess.DateAdded = DateTime.UtcNow;

                    // Check this point has not already been awarded
                    var canAddPoints = true;

                    // Check to see if this has an id
                    if (input.EntityToProcess.PointsForId != null)
                    {
                        var alreadyHasThisPoint = _membershipUserPointsService.GetByUser(input.EntityToProcess.User)
                                .Any(x => x.PointsFor == input.EntityToProcess.PointsFor && x.PointsForId == input.EntityToProcess.PointsForId);

                        canAddPoints = (alreadyHasThisPoint == false);
                    }

                    // If they can ad points let them
                    if (canAddPoints)
                    {
                       context.MembershipUserPoints.Add(input.EntityToProcess);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}