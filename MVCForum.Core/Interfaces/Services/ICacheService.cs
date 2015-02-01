namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ICacheService
    {
        object Get(string key);
        void Set(string key, object data, int cacheTime);
        bool IsSet(string key);
        void Invalidate(string key);
    }
}
