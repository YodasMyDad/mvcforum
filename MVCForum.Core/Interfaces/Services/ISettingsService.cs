using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ISettingsService
    {
        Settings GetSettings(bool useCache = true);
        Settings Add(Settings settings);
    }
}
