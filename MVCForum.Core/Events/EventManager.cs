namespace MvcForum.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Interfaces.Events;
    using Interfaces.Services;
    using Reflection;
    using Utilities;

    public sealed class EventManager : IEventManager
    {
        private const string InterfaceTargetName = @"MvcForum.Core.Interfaces.Events.IEventHandler";

        private static volatile EventManager _instance;

        private static readonly object SyncRoot = new object();

        public EventHandler<BadgeEventArgs> AfterBadgeAwarded;
        public EventHandler<FavouriteEventArgs> AfterFavourite;
        public EventHandler<LoginEventArgs> AfterLogin;
        public EventHandler<MarkedAsSolutionEventArgs> AfterMarkedAsSolution;
        public EventHandler<PostMadeEventArgs> AfterPostMade;
        public EventHandler<PrivateMessageEventArgs> AfterPrivateMessage;
        public EventHandler<TopicMadeEventArgs> AfterTopicMade;
        public EventHandler<UpdateProfileEventArgs> AfterUpdateProfile;
        public EventHandler<VoteEventArgs> AfterVoteMade;
        public EventHandler<BadgeEventArgs> BeforeBadgeAwarded;
        public EventHandler<FavouriteEventArgs> BeforeFavourite;
        public EventHandler<LoginEventArgs> BeforeLogin;
        public EventHandler<MarkedAsSolutionEventArgs> BeforeMarkedAsSolution;
        public EventHandler<PostMadeEventArgs> BeforePostMade;
        public EventHandler<PrivateMessageEventArgs> BeforePrivateMessage;
        public EventHandler<TopicMadeEventArgs> BeforeTopicMade;
        public EventHandler<UpdateProfileEventArgs> BeforeUpdateProfile;
        public EventHandler<VoteEventArgs> BeforeVoteMade;

        public ILoggingService Logger { get; set; }

        /// <summary>
        ///     Singleton instance
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

        #region Initialise Code

        /// <summary>
        ///     Use reflection to get all event handling classes. Call this ONCE.
        /// </summary>
        public void Initialize(ILoggingService loggingService, List<Assembly> assemblies)
        {
            Logger = loggingService;

            var interfaceFilter = new TypeFilter(ImplementationManager.InterfaceFilter);

            foreach (var nextAssembly in assemblies)
            {
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
                        $"Unable to load assembly. Probably not an event assembly, loader exception was: '{rtle.LoaderExceptions[0].GetType()}':'{rtle.LoaderExceptions[0].Message}'.";
                    LogError(msg);
                }
                catch (Exception ex)
                {
                    LogError($"Error reflecting over event handlers: {ex.Message}");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Log errors
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(string msg)
        {
            Logger?.Error(msg);
        }

        #region Badges

        public void FireAfterBadgeAwarded(object sender, BadgeEventArgs eventArgs)
        {
            AfterBadgeAwarded?.Invoke(this, eventArgs);
        }

        public void FireBeforeBadgeAwarded(object sender, BadgeEventArgs eventArgs)
        {
            BeforeBadgeAwarded?.Invoke(this, eventArgs);
        }

        #endregion

        #region Votes

        public void FireBeforeVoteMade(object sender, VoteEventArgs eventArgs)
        {
            BeforeVoteMade?.Invoke(this, eventArgs);
        }

        public void FireAfterVoteMade(object sender, VoteEventArgs eventArgs)
        {
            AfterVoteMade?.Invoke(this, eventArgs);
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

    }
}