namespace MvcForum.Core.Pipeline
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Pipeline;
    using Models.Entities;

    /// <summary>
    ///     Main pipeline that deals with looping through each pipe and returning a PipeLineProcess
    /// </summary>
    /// <typeparam name="TOne"></typeparam>
    /// <typeparam name="TTwo"></typeparam>
    public class Pipeline<TOne, TTwo>
        where TTwo : IBaseEntity
        where TOne : IPipelineProcess<TTwo>
    {
        private readonly IMvcForumContext _context;

        //we'll store the registered actions here
        private readonly List<IPipe<TOne>> _pipeline = new List<IPipe<TOne>>();

        public Pipeline(IMvcForumContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Register the pipes
        /// </summary>
        /// <param name="pipe"></param>
        public void Register(IPipe<TOne> pipe)
        {
            _pipeline.Add(pipe);
        }

        /// <summary>
        ///     Optional remove a pipe
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _pipeline.RemoveAt(index);
        }

        /// <summary>
        ///     Process the pipes and return the pipelineprocess
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TOne> Process(TOne model)
        {
            foreach (var pipe in _pipeline)
            {
                if (model.Successful)
                {
                    await pipe.Process(model, _context);
                }
            }
            return model;
        }
    }
}