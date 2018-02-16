namespace MvcForum.Core.Interfaces
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///     Describes an assembly provider with the mechanism of getting assemblies that should be involved
    ///     in the types discovery process.
    /// </summary>
    public interface IAssemblyProvider
    {
        /// <summary>
        ///     Discovers and then gets the discovered assemblies.
        /// </summary>
        /// <param name="path">
        ///     The extensions path of a web application. Might be used or ignored
        ///     by an implementation of the <see cref="IAssemblyProvider">IAssemblyProvider</see> interface.
        /// </param>
        /// <returns></returns>
        IEnumerable<Assembly> GetAssemblies(string path);

        /// <summary>
        ///     Discovers and then gets the discovered assemblies.
        /// </summary>
        /// <param name="paths">
        ///     Takes in multiple paths
        /// </param>
        /// <returns></returns>
        IEnumerable<Assembly> GetAssemblies(IEnumerable<string> paths);
    }
}