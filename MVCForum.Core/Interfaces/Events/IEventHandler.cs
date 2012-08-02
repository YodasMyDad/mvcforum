using MVCForum.Domain.Events;

namespace MVCForum.Domain.Interfaces.Events
{
    public interface IEventHandler
    {
        void RegisterHandlers(EventManager theEventManager);
    }
}
