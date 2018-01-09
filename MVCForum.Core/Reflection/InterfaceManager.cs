namespace MvcForum.Core.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class InterfaceManager
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
            return GetInstance<T>(null, new object[] { });
        }

        /// <summary>
        ///     Gets the new instance (using constructor that matches the arguments) of the first implementation
        ///     of the type specified by the type parameter or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="args">The arguments to be passed to the constructor.</param>
        /// <returns></returns>
        public static KeyValuePair<string, T> GetInstance<T>(params object[] args)
        {
            return GetInstance<T>(null, args);
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
        ///     Gets the new instance (using constructor that matches the arguments) of the first implementation
        ///     of the type specified by the type parameter and located in the assemblies filtered by the predicate
        ///     or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <param name="args">The arguments to be passed to the constructor.</param>
        /// <returns></returns>
        public static KeyValuePair<string, T> GetInstance<T>(Func<Assembly, bool> predicate, params object[] args)
        {
            return GetInstances<T>(predicate, args).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the new instances of the implementations of the type specified by the type parameter
        ///     or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <returns></returns>
        public static Dictionary<string, T> GetInstances<T>()
        {
            return GetInstances<T>(null, new object[] { });
        }

        /// <summary>
        ///     Gets the new instances (using constructor that matches the arguments) of the implementations
        ///     of the type specified by the type parameter or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="args">The arguments to be passed to the constructors.</param>
        /// <returns></returns>
        public static Dictionary<string, T> GetInstances<T>(params object[] args)
        {
            return GetInstances<T>(null, args);
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
            return GetInstances<T>(predicate, new object[] { });
        }

        /// <summary>
        ///     Gets the new instances (using constructor that matches the arguments) of the implementations
        ///     of the type specified by the type parameter and located in the assemblies filtered by the predicate
        ///     or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// <param name="args">The arguments to be passed to the constructors.</param>
        /// <returns></returns>
        public static Dictionary<string, T> GetInstances<T>(Func<Assembly, bool> predicate, params object[] args)
        {
            var instances = new Dictionary<string, T>();

            foreach (var implementation in GetImplementations<T>())
            {
                if (!implementation.GetTypeInfo().IsAbstract)
                {
                    var instance = (T) Activator.CreateInstance(implementation, args);
                    instances.Add(implementation.FullName, instance);
                }
            }

            return instances;
        }

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