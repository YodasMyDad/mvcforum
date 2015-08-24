using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Domain.Events
{
    public sealed class EventManager : IEventManager
    {
        private const string InterfaceTargetName =@"MVCForum.Domain.Interfaces.Events.IEventHandler";

        public EventHandler<BadgeEventArgs> BeforeBadgeAwarded;
        public EventHandler<BadgeEventArgs> AfterBadgeAwarded;

        public EventHandler<VoteEventArgs> BeforeVoteMade;
        public EventHandler<VoteEventArgs> AfterVoteMade;

        public EventHandler<MarkedAsSolutionEventArgs> BeforeMarkedAsSolution;
        public EventHandler<MarkedAsSolutionEventArgs> AfterMarkedAsSolution;

        public EventHandler<PostMadeEventArgs> BeforePostMade;
        public EventHandler<PostMadeEventArgs> AfterPostMade;

        public EventHandler<RegisterUserEventArgs> BeforeRegisterUser;
        public EventHandler<RegisterUserEventArgs> AfterRegisterUser;


        public EventHandler<UpdateProfileEventArgs> BeforeUpdateProfile;
        public EventHandler<UpdateProfileEventArgs> AfterUpdateProfile;

        public EventHandler<LoginEventArgs> BeforeLogin;
        public EventHandler<LoginEventArgs> AfterLogin;

        public EventHandler<FavouriteEventArgs> BeforeFavourite;
        public EventHandler<FavouriteEventArgs> AfterFavourite;

        public EventHandler<PrivateMessageEventArgs> BeforePrivateMessage;
        public EventHandler<PrivateMessageEventArgs> AfterPrivateMessage;

        private static volatile EventManager _instance;
        private static readonly object SyncRoot = new Object();

        public ILoggingService Logger { get; set; }

        /// <summary>
        /// Constructor - hidden
        /// </summary>
        private EventManager()
        {
        }

        /// <summary>
        /// Callback used when comparing objects to see if they implement an interface
        /// </summary>
        /// <param name="typeObj"></param>
        /// <param name="criteriaObj"></param>
        /// <returns></returns>
        private static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new EventManager();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Log errors
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(string msg)
        {
            if (Logger != null)
            {
                Logger.Error(msg);
            }
        }

        #region Initialise Code
        /// <summary>
        /// Use reflection to get all event handling classes. Call this ONCE.
        /// </summary>
        public void Initialize(ILoggingService loggingService)
        {
            Logger = loggingService;

            var interfaceFilter = new TypeFilter(InterfaceFilter);

            var path = AppDomain.CurrentDomain.RelativeSearchPath;

            // Get all the dlls
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.dll"))
            {

                var dllsToSkip = AppConstants.ReflectionDllsToAvoid;
                //if (file.Name == "EcmaScript.NET.dll" || file.Name == "Unity.WebApi.dll")
                if (dllsToSkip.Contains(file.Name))
                {
                    continue;
                }
                //if (file.Name.ToLower().StartsWith("ecmascript") || file.Name.ToLower().StartsWith("unity."))
                //{
                //    continue;
                //}

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

                if (nextAssembly.FullName.StartsWith("System") || nextAssembly.FullName.StartsWith("Microsoft") || nextAssembly.FullName.StartsWith("DotNetOpenAuth") || nextAssembly.FullName.StartsWith("Unity"))
                {
                    // Skip microsoft assemblies
                    continue;
                }

                try
                {
                    foreach (var type in nextAssembly.GetTypes())
                    {
                        if (type.IsInterface)
                        {
                            continue;
                        }

                        var myInterfaces = type.FindInterfaces(interfaceFilter, InterfaceTargetName);
                        if (myInterfaces.Length <= 0)
                        {
                            // Not a match
                            continue;
                        }

                        var ctor = type.GetConstructors().First();
                        var createdActivator = ReflectionUtilities.GetActivator<IEventHandler>(ctor);

                        // Create an instance:
                        var instance = createdActivator();

                        instance.RegisterHandlers(this);
                    }
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    var msg =
                        string.Format(
                            "Unable to load assembly. Probably not an event assembly. In file named '{0}', loader exception was: '{1}':'{2}'.",
                            file.Name, rtle.LoaderExceptions[0].GetType(), rtle.LoaderExceptions[0].Message);
                    LogError(msg);
                }
                catch (Exception ex)
                {
                    LogError(string.Format("Error reflecting over event handlers: {0}", ex.Message));
                }
            }
        } 
        #endregion

        #region Badges
        public void FireAfterBadgeAwarded(object sender, BadgeEventArgs eventArgs)
        {
            var handler = AfterBadgeAwarded;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireBeforeBadgeAwarded(object sender, BadgeEventArgs eventArgs)
        {
            var handler = BeforeBadgeAwarded;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Votes
        public void FireBeforeVoteMade(object sender, VoteEventArgs eventArgs)
        {
            var handler = BeforeVoteMade;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireAfterVoteMade(object sender, VoteEventArgs eventArgs)
        {
            var handler = AfterVoteMade;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Solutions
        public void FireBeforeMarkedAsSolution(object sender, MarkedAsSolutionEventArgs eventArgs)
        {
            var handler = BeforeMarkedAsSolution;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireAfterMarkedAsSolution(object sender, MarkedAsSolutionEventArgs eventArgs)
        {
            var handler = AfterMarkedAsSolution;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Posts
        public void FireBeforePostMade(object sender, PostMadeEventArgs eventArgs)
        {
            var handler = BeforePostMade;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
        public void FireAfterPostMade(object sender, PostMadeEventArgs eventArgs)
        {
            var handler = AfterPostMade;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Profile
        public void FireBeforeProfileUpdated(object sender, UpdateProfileEventArgs eventArgs)
        {
            var handler = BeforeUpdateProfile;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
        public void FireAfterProfileUpdated(object sender, UpdateProfileEventArgs eventArgs)
        {
            var handler = AfterUpdateProfile;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Register
        public void FireBeforeRegisterUser(object sender, RegisterUserEventArgs eventArgs)
        {
            var handler = BeforeRegisterUser;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireAfterRegisterUser(object sender, RegisterUserEventArgs eventArgs)
        {
            var handler = AfterRegisterUser;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        } 
        #endregion

        #region Favourites

        public void FireAfterFavourite(object sender, FavouriteEventArgs eventArgs)
        {
            var handler = AfterFavourite;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireBeforeFavourite(object sender, FavouriteEventArgs eventArgs)
        {
            var handler = BeforeFavourite;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        #endregion

        #region Private Messages

        public void FireAfterPrivateMessage(object sender, PrivateMessageEventArgs eventArgs)
        {
            var handler = AfterPrivateMessage;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireBeforePrivateMessage(object sender, PrivateMessageEventArgs eventArgs)
        {
            var handler = BeforePrivateMessage;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        #endregion

        #region Login
        public void FireBeforeLogin(object sender, LoginEventArgs eventArgs)
        {
            var handler = BeforeLogin;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void FireAfterLogin(object sender, LoginEventArgs eventArgs)
        {
            var handler = AfterLogin;

            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
        #endregion
    }
}
