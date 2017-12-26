namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using DomainModel.Entities;

    public interface ISettingsService
    {
        Settings GetSettings(bool useCache = true);
        Settings Add(Settings settings);
        Settings Get(Guid id);
    }
}