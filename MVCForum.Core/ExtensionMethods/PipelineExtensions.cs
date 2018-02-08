namespace MvcForum.Core.ExtensionMethods
{
    using Interfaces;
    using Interfaces.Pipeline;
    using Models.Entities;

    public static class PipelineExtensions
    {
        /// <summary>
        /// Quick way of adding error message during pipeline 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="process"></param>
        /// <param name="errorMessage"></param>
        /// <param name="clearLog"></param>
        public static void AddError<T>(this IPipelineProcess<T> process, string errorMessage, bool clearLog = true)
            where T : IBaseEntity
        {
            if (clearLog)
            {
                process.ProcessLog.Clear();
            }
            process.Successful = false;
            process.ProcessLog.Add(errorMessage);
        }
    }
}