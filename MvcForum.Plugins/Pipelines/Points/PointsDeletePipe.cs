namespace MvcForum.Plugins.Pipelines.Points
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class PointsDeletePipe : IPipe<IPipelineProcess<MembershipUserPoints>>
    {
        /// <inheritdoc />
        public Task<IPipelineProcess<MembershipUserPoints>> Process(IPipelineProcess<MembershipUserPoints> input,
            IMvcForumContext context)
        {
            throw new NotImplementedException();
        }
    }
}