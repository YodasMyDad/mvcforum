namespace MvcForum.Plugins.Pipelines.Points
{
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class PointsDeletePipe : IPipe<IPipelineProcess<MembershipUserPoints>>
    {
        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUserPoints>> Process(IPipelineProcess<MembershipUserPoints> input,
            IMvcForumContext context)
        {
            if (input.EntityToProcess != null)
            {
                context.MembershipUserPoints.Remove(input.EntityToProcess);
                await context.SaveChangesAsync();
            }

            return input;
        }
    }
}