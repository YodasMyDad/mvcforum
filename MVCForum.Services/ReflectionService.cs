using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class ReflectionService : IReflectionService
    {
        /// <summary>
        /// Callback used when comparing objects to see if they implement an interface
        /// </summary>
        /// <param name="typeObj"></param>
        /// <param name="criteriaObj"></param>
        /// <returns></returns>
        public bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        public List<Assembly> GetAssemblies()
        {
            //var interfaceFilter = new TypeFilter(InterfaceFilter);
            var assemblies = new List<Assembly>();
            var path = AppDomain.CurrentDomain.RelativeSearchPath;

            // Get all the dlls
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.dll"))
            {

                var dllsToSkip = AppConstants.ReflectionDllsToAvoid;
                if (dllsToSkip.Contains(file.Name))
                {
                    continue;
                }

                Assembly nextAssembly;
                try
                {
                    nextAssembly = Assembly.LoadFrom(file.FullName);
                }
                catch (BadImageFormatException)
                {
                    // Not an assembly ignore
                    continue;
                }
                assemblies.Add(nextAssembly);
                
            }

            return assemblies;
        }
    }
}
