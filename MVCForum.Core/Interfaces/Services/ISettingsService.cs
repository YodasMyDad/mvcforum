namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using Models.Entities;

    public partial interface ISettingsService : IContextService
    {
        Settings GetSettings(bool useCache = true);
        Settings Add(Settings settings);
        Settings Get(Guid id);
    }
}