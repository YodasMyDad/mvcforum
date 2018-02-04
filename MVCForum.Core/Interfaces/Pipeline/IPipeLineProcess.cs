namespace MvcForum.Core.Interfaces.Pipeline
{
    using System.Collections.Generic;
    using Models;
    using Models.Entities;

    public interface IPipelineProcess<T> where T : IBaseEntity
    {
        /// <summary>
        ///     The entity to create, update or delete
        /// </summary>
        T EntityToProcess { get; set; }

        /// <summary>
        ///     The log of whats happened
        /// </summary>
        List<string> ProcessLog { get; set; }

        /// <summary>
        ///     Was this pipeline process successful
        /// </summary>
        bool Successful { get; set; }

        /// <summary>
        ///     Extended data list to store any extra data we need to use through the pipelines
        /// </summary>
        Dictionary<string, object> ExtendedData { get; set; }
    }
}