namespace MvcForum.Core.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;
    using Interfaces;
    using Interfaces.Services;

    public class AssemblyProvider : IAssemblyProvider
    {
        private readonly ILoggingService _loggingService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyProvider">AssemblyProvider</see> class.
        /// </summary>
        public AssemblyProvider(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            IsMvcForumCandidateAssembly = assembly => assembly != null;
        }

        /// <summary>
        ///     Gets or sets the predicate that is used to filter discovered assemblies from a specific folder
        ///     before thay have been added to the resulting assemblies set.
        /// </summary>
        public Func<Assembly, bool> IsMvcForumCandidateAssembly { get; set; }


        /// <inheritdoc />
        public IEnumerable<Assembly> GetAssemblies(string path)
        {
            var assemblies = new List<Assembly>();
            assemblies.AddRange(FindAssemblies(path));
            return assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<Assembly> GetAssemblies(IEnumerable<string> paths)
        {
            var assemblies = new List<Assembly>();
            foreach (var path in paths)
            {
                assemblies.AddRange(FindAssemblies(path));
            }
            return assemblies;
        }

        /// <summary>
        ///     Helper method to get all the assemblies from a path or the root of tha aplication
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<Assembly> FindAssemblies(string path)
        {
            var assemblies = new List<Assembly>();

            if (!string.IsNullOrEmpty(path))
            {
                var folderPath = new DirectoryInfo(HostingEnvironment.MapPath(path));
                foreach (var file in folderPath.GetFileSystemInfos("*.dll", SearchOption.AllDirectories))
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.LoadFile(file.FullName);
                    }
                    catch (FileLoadException)
                    {
                        // Get loaded assembly
                        assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(file.Name)));

                        if (assembly == null)
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(ex, "Error when trying to get assemblies via FindAssemblies");
                    }

                    if (assembly != null)
                    {
                        assemblies.Add(assembly);
                    }
                }
            }
            else
            {
                // Empty path so just grab the assemblies from the application
                foreach (var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load))
                {
                    if (IsMvcForumCandidateAssembly(assembly))
                    {
                        try
                        {
                            assemblies.Add(assembly);
                        }

                        catch (Exception ex)
                        {
                            _loggingService.Error(ex, $"Error loading assembly '{assembly.FullName}'");
                        }
                    }
                }
            }

            return assemblies;
        }
    }
}