namespace MvcForum.Core.Interfaces.Events
{
    using Core.Events;

    public partial interface IEventHandler
    {
        void RegisterHandlers(EventManager theEventManager);
    }
}