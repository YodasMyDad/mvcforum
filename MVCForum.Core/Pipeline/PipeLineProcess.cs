namespace MvcForum.Core.Pipeline
{
    using System.Collections.Generic;
    using Interfaces.Pipeline;
    using Models;
    using Models.Entities;

    /// <inheritdoc />
    public class PipelineProcess<T> : IPipelineProcess<T>
        where T : IBaseEntity
    {

        public PipelineProcess(T entity)
        {
            // Set up the pipeline process
            EntityToProcess = entity;
            ProcessLog = new List<string>();
            ExtendedData = new Dictionary<string, string>();
            Successful = true;
        }

        /// <inheritdoc />
        public T EntityToProcess { get; set; }

        /// <inheritdoc />
        public bool Successful { get; set; }

        /// <inheritdoc />
        public Dictionary<string, string> ExtendedData { get; set; }

        /// <inheritdoc />
        public List<string> ProcessLog { get; set; }
    }
}