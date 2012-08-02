using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface ISettingsRepository
    {
        Settings GetSettings();

        Settings Add(Settings item);
        Settings Get(Guid id);
        void Update(Settings item);
    }
}
