using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.Controllers;
using MVCForum.Website.Areas.Admin.ViewModels;
using NUnit.Framework;
using NSubstitute;

namespace MVCForum.Tests.Controller_Tests
{
    public class LocalizationControllerTests
    {
        private AdminLanguageController _adminLanguageController;
        private ILocalizationService _localizationServiceSub;
        private IUnitOfWorkManager _unitOfWorkManagerSub;


        [SetUp]
        public void Init()
        {
            var settingsSub = Substitute.For<ISettingsService>();
            var membershipSub = Substitute.For<IMembershipService>();
            var loggingService = Substitute.For<ILoggingService>();
            _localizationServiceSub = Substitute.For<ILocalizationService>();
            _unitOfWorkManagerSub = Substitute.For<IUnitOfWorkManager>();

            _adminLanguageController = new AdminLanguageController(loggingService, _unitOfWorkManagerSub, membershipSub, _localizationServiceSub, settingsSub);
        }

        [Test]
        public void GetLanguages()
        {
            var languages = new List<Language>{
                    new Language
                            {
                                Id = Guid.NewGuid(),
                                LanguageCulture = "de-AT",
                                Name = "German Austrian",                                
                            },
                    new Language
                            {
                                Id = Guid.NewGuid(),
                                LanguageCulture = "de-DE",
                                Name = "German German",
                            }};

            _localizationServiceSub.AllLanguages.Returns(languages);
            _localizationServiceSub.CurrentLanguage.Returns(new Language {Name = "DummyLang"});
            var partialViewResult = _adminLanguageController.GetLanguages();

            var vm = (ListLanguagesViewModel) partialViewResult.Model;
            
            Assert.IsTrue(vm.Languages.Count == 2);
        }
    }
}
