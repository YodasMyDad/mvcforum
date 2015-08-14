using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ISettingsRepository
    {
        Settings GetSettings(bool addTracking);
        Settings Add(Settings item);
        Settings Get(Guid id);
    }
}
