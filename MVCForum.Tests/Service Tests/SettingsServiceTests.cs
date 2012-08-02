using System;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    //[TestFixture]
    //public class SettingsServiceTests
    //{

    //    [Test]
    //    public void Check_Settings_IsIn_Cache()
    //    {
    //        var settingsRepository = Substitute.For<ISettingsRepository>();
    //        var settingsService = new SettingsService(settingsRepository, _testCacheHelper);

    //        var settings = new Settings { Id = Guid.NewGuid() };
    //        settingsRepository.GetSettings().Returns(settings);

    //        // Save
    //        settingsService.GetSettings();

    //        // Test that settings is no longer in cache
    //        Assert.IsTrue(_testCacheHelper.Exists(AppConstants.SettingsCacheName));
    //    }
    //}
}
