namespace MvcForum.Core.Interfaces.Events
{
    using System.Collections.Generic;
    using System.Reflection;
    using Services;

    public partial interface IEventManager
    {
        /// <summary>
        ///     Use reflection to get all event handling classes. Call this ONCE.
        /// </summary>
        void Initialize(ILoggingService loggingService, List<Assembly> assemblies);
    }
}