using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ISettingsService
    {
        Settings GetSettings(bool useCache = true);
        void Save(Settings settings);
        Settings Add(Settings settings);
    }
}
