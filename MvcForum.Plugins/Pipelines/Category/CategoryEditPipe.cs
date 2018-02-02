namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class CategoryEditPipe : IPipe<IPipelineProcess<Category>>
    {
        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input, IMvcForumContext context)
        {

            return input;
        }
    }
}