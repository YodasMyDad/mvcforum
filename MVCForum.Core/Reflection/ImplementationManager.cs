namespace MvcForum.Core.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Ioc;
    using Unity;

    public static class ImplementationManager
    {
        /// <summary>
        ///     Gets the cached assemblies that has been set by the <c>SetAssemblies</c> method.
        /// </summary>
        public static IEnumerable<Assembly> Assemblies { get; private set; }

        /// <summary>
        ///     Gets the plugins and their data
        /// </summary>
        /// <summary>
        ///     Sets all the assemblies
        /// </summary>
        /// <param name="assemblies">The assemblies to set.</param>
        public static void SetAssemblies(IEnumerable<Assembly> assemblies)
        {
            Assemblies = assemblies;
        }

        /// <summary>
        ///     Gets the first implementation of the type specified by the type parameter or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <returns></returns>
        public static Type GetImplementation<T>()
        {
            return GetImplementation<T>(null);
        }

        /// <summary>
        ///     Gets the first implementation of the type specified by the type parameter and located in the assemblies
        ///     filtered by the predicate or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <returns></returns>
        public static Type GetImplementation<T>(Func<Assembly, bool> predicate)
        {
            return GetImplementations<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the implementations of the type specified by the type parameter.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementations<T>()
        {
            return GetImplementations<T>(null);
        }

        /// <summary>
        ///     Gets the implementations of the type specified by the type parameter and located in the assemblies
        ///     filtered by the predicate.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementations<T>(Func<Assembly, bool> predicate)
        {
            var implementations = new List<Type>();

            foreach (var assembly in GetAssemblies(predicate))
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(T).GetTypeInfo().IsAssignableFrom(type) && type.GetTypeInfo().IsClass)
                {
                    implementations.Add(type);
                }
            }

            return implementations;
        }

        /// <summary>
        ///     Gets the new instance of the first implementation of the type specified by the type parameter
        ///     or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <returns></returns>
        public static KeyValuePair<string, T> GetInstance<T>()
        {
            return GetInstance<T>(null);
        }


        /// <summary>
        ///     Gets the new instance of the first implementation of the type specified by the type parameter
        ///     and located in the assemblies filtered by the predicate or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <returns></returns>
        public static KeyValuePair<string, T> GetInstance<T>(Func<Assembly, bool> predicate)
        {
            return GetInstances<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the new instances of the implementations of the type specified by the type parameter
        ///     or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <returns></returns>
        public static Dictionary<string, T> GetInstances<T>()
        {
            return GetInstances<T>(null);
        }

        /// <summary>
        ///     Gets the new instances of the implementations of the type specified by the type parameter
        ///     and located in the assemblies filtered by the predicate or empty enumeration
        ///     if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <returns></returns>
        public static Dictionary<string, T> GetInstances<T>(Func<Assembly, bool> predicate)
        {
            var instances = new Dictionary<string, T>();

            foreach (var implementation in GetImplementations<T>())
            {
                if (!implementation.GetTypeInfo().IsAbstract)
                {
                    var fullName = implementation.FullName ?? "MissingFullName";
                    var instance = (T)UnityHelper.Container.Resolve(implementation);    
                    instances.Add(fullName, instance);
                }
            }

            return instances;
        }

        /// <summary>
        /// Callback used when comparing objects to see if they implement an interface
        /// </summary>
        /// <param name="typeObj"></param>
        /// <param name="criteriaObj"></param>
        /// <returns></returns>
        public static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        /// <summary>
        ///     Get the specified attribute off a badge class
        /// </summary>
        /// <param name="type">The class type</param>
        /// <returns>The attribute class instance</returns>
        public static T GetAttribute<T>(Type type) where T : class
        {
            foreach (var attribute in type.GetCustomAttributes(false))
            {
                if (attribute is T)
                {
                    return attribute as T;
                }
            }

            throw new Exception("Attribute not found");
        }

        /// <summary>
        /// Get Assemblies
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static IEnumerable<Assembly> GetAssemblies(Func<Assembly, bool> predicate)
        {
            if (predicate == null)
            {
                return Assemblies;
            }

            return Assemblies.Where(predicate);
        }
    }
}