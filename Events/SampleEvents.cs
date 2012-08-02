using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Events;

namespace Events
{
    public class SampleEvents : IEventHandler
    {
        public void RegisterHandlers(EventManager theEventManager)
        {
            theEventManager.BeforeBadgeAwarded += BeforeBadgeAwardedHandler;
        }

        private void BeforeBadgeAwardedHandler(object sender, BadgeEventArgs eventArgs)
        {
            // Do something

        }

    }
}
