using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ISettingsService
    {
        Settings GetSettings();
        void Save(Settings settings);
    }
}
