using System;
using System.Collections.Generic;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Utilities;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class LocalizationServiceTests
    {
        private ILocalizationService _localizationService;
    
        private ILocalizationRepository _localizationRepositorySub;
        private ISettingsRepository _settingsRepositorySub;
        

        [SetUp]
        public void Init()
        {            
            var unitOfWorkManagerSub = Substitute.For<IUnitOfWorkManager>();            
            var unitOfWorkSub = Substitute.For<IUnitOfWork>();
            
            _localizationRepositorySub = Substitute.For<ILocalizationRepository>();
            _settingsRepositorySub = Substitute.For<ISettingsRepository>();
            var loggingService = Substitute.For<ILoggingService>();
            unitOfWorkManagerSub.NewUnitOfWork().Returns(unitOfWorkSub);

            _localizationService = new LocalizationService(_localizationRepositorySub, _settingsRepositorySub, loggingService);
        }
        
        [Test]
        public void DeleteDefaultLanguage()
        {
            var testLanguage = new Language {Name = "test"};
            var settings = new Settings {DefaultLanguage = testLanguage};

            _settingsRepositorySub.GetSettings().Returns(settings);

            Assert.Throws<ApplicationException>(() => _localizationService.Delete(testLanguage));
        }

        //[Test]
        //public void DeleteLanguageInCache()
        //{
        //    var language = new Language{ Id = Guid.NewGuid(), Name = "test"};

        //    // Put language in cache
        //    _testCacheHelper.Clear(AppConstants.LocalizationCacheName);
        //    _testCacheHelper.Add(language, AppConstants.LocalizationCacheName);

        //    _localizationService.Delete(language);

        //    // Test that language is no longer in cache
        //    Assert.IsFalse(_testCacheHelper.Exists(AppConstants.LocalizationCacheName));          
        //}

        //[Test]
        //public void DeleteLanguageException()
        //{
        //    var language = new Language { Id = Guid.NewGuid(), Name = "test" };
          
        //    _localizationRepositorySub.When(x => x.Delete(language))
        //        .Do(x => { throw new Exception(); });

        //    try
        //    {
        //        _localizationService.Delete(language);
        //    }
        //    catch 
        //    {
        //       // Do nothing as we forced this
        //    }            

        //    // The exception on delete should have restored the cache
        //    Assert.IsTrue(_testCacheHelper.Exists(AppConstants.LocalizationCacheName)); 
        //}

        //[Test]
        //public void SaveWhenLanguageInCache()
        //{
        //    var language = new Language { Id = Guid.NewGuid(), Name = "test" };
        //    _testCacheHelper.Clear(AppConstants.LocalizationCacheName);
        //    _testCacheHelper.Add(language, AppConstants.LocalizationCacheName);

        //    language.Name = "test2";

        //    _localizationService.Save(language);

        //    Language langRetrieved; 
        //    _testCacheHelper.Get(AppConstants.LocalizationCacheName, out langRetrieved);

        //    // Cache should be updated
        //    Assert.AreEqual(langRetrieved.Name, "test2");
        //}

        //[Test]
        //public void SaveWhenLanguageNotInCache()
        //{
        //    var language = new Language { Id = Guid.NewGuid(), Name = "test" };
        //    var language2 = new Language { Id = Guid.NewGuid(), Name = "testInCache" };
        //    _testCacheHelper.Clear(AppConstants.LocalizationCacheName);
        //    _testCacheHelper.Add(language2, AppConstants.LocalizationCacheName);
        //    _localizationService.Save(language);

        //    Language langRetrieved;
        //    _testCacheHelper.Get(AppConstants.LocalizationCacheName, out langRetrieved);

        //    // Cache should be updated
        //    Assert.AreEqual(langRetrieved.Name, "testInCache");
        //}

        //[Test]
        //public void DeleteResourceKey()
        //{
        //    var language = new Language { Id = Guid.NewGuid(), Name = "test" };
        //    var resourceKey = new LocaleResourceKey();
        //    _testCacheHelper.Clear(AppConstants.LocalizationCacheName);
        //    _testCacheHelper.Add(language, AppConstants.LocalizationCacheName);
        //    _localizationService.DeleteLocaleResourceKey(resourceKey);

        //    Language langRetrieved;
        //    var inCache = _testCacheHelper.Get(AppConstants.LocalizationCacheName, out langRetrieved);

        //    // Cache should be updated - language entry removed
        //    Assert.IsFalse(inCache);
        //}

        [Test]
        public void LanguagesInDb()
        {
            var languages = new List<Language>{
                    new Language
                            {
                                LanguageCulture = "de-AT"
                            },
                    new Language
                            {
                                LanguageCulture = "de-DE"
                            }};

            _localizationRepositorySub.GetAll().Returns(languages);

            var langsInDb = _localizationService.LanguagesInDb;

            Assert.AreEqual(langsInDb.Count, 2);
        }

        [Test]
        public void LanguagesNotInDb()
        {
            var languages = new List<Language>{
                    new Language
                            {
                                LanguageCulture = "de-AT"
                            },
                    new Language
                            {
                                LanguageCulture = "de-DE"
                            }};


            _localizationRepositorySub.GetAll().Returns(languages);

            var langNotsInDb = _localizationService.LanguagesNotInDb;
            
            Assert.AreEqual(langNotsInDb.Count + 2, LanguageUtils.Count);
        }

        #region Import from CSV

        /// <summary>
        /// Badly formatted language and culture strings in CSV should throw an error
        /// </summary>
        [Test]
        public void ImportCsvNonExistantLanguageCulture()
        {
            var report = _localizationService.FromCsv("frFR", new List<string> { "test", "test"});

            Assert.AreEqual(report.Errors.Count, 1);
            Assert.AreEqual(report.Errors[0].ErrorWarningType, CsvErrorWarningType.DoesNotExist);
       }

        /// <summary>
        /// Unreadable data should throw an error
        /// </summary>
        [Test]
        public void ImportCsvBadData()
        {
            _localizationRepositorySub.GetLanguageByLanguageCulture("fr-FR").Returns(x => null);

            var report = _localizationService.FromCsv("fr-FR", null);

            Assert.AreEqual(report.Errors.Count, 1);
            Assert.AreEqual(report.Errors[0].ErrorWarningType, CsvErrorWarningType.BadDataFormat);
       }

        /// <summary>
        /// Positive case - import a language
        /// </summary>
        [Test]
        public void ImportLanguage()
        {
            // Ensure language does not exist
            _localizationRepositorySub.GetLanguageByLanguageCulture(Arg.Any<string>()).Returns(x => null);

            var resourceKey1 = new LocaleResourceKey
                                   {
                                       DateAdded = DateTime.UtcNow,
                                       Id = Guid.NewGuid(),
                                       Name = "testKey1",
                                       Notes = "test notes"
                                   };

            var resourceValue1 = new LocaleStringResource
                                     {
                                         LocaleResourceKey = resourceKey1,
                                         ResourceValue = "testValue1"
                                     };

            var resourceKey2 = new LocaleResourceKey
                                   {
                                       DateAdded = DateTime.UtcNow,
                                       Id = Guid.NewGuid(),
                                       Name = "testKey2",
                                       Notes = "test notes"
                                   };

            var resourceValue2 = new LocaleStringResource
            {
                LocaleResourceKey = resourceKey1,
                ResourceValue = "testValue2"
            };

            var newLanguage = new Language
            {
                LanguageCulture = "fr-FR",
                Name = "French",
                LocaleStringResources = new List<LocaleStringResource> { resourceValue1, resourceValue2 },
            };

            _localizationRepositorySub.GetAllResourceKeys().Returns(new List<LocaleResourceKey> { resourceKey1, resourceKey2 });

            _localizationRepositorySub.GetResourceKey("testKey1").Returns(resourceKey1);

            _localizationRepositorySub.GetResourceKey("testKey2").Returns(resourceKey2);

            _localizationRepositorySub.GetResourceKey("testKeyNew").Returns(x => null);

            _localizationRepositorySub.GetAll().Returns(new List<Language> { newLanguage });

            var testData = new List<string>
                                {
                                    "testKey1,testValue1",
                                    "testKey2,testValue2",
                                    ",should not import", // Test for ignore of empty key
                                    "testKeyNew,testValueNew"
                                };


            var report = _localizationService.FromCsv("fr-FR", testData);

            Assert.AreEqual(report.Warnings.Count, 1);
            Assert.AreEqual(report.Warnings[0].ErrorWarningType, CsvErrorWarningType.NewKeyCreated);
            Assert.AreEqual(report.Errors.Count, 0);
        }

        #endregion

        #region Export to CSV

        /// <summary>
        /// Positive test
        /// </summary>
        [Test]
        public void ExportCsv()
        {
            var testGuid = Guid.NewGuid();

            var resourceKey1 = new LocaleResourceKey
                                   {
                                       DateAdded = DateTime.UtcNow,
                                       Id = Guid.NewGuid(),
                                       Name = "testKey1",
                                       Notes = "test notes"
                                   };

            var resourceValue1 = new LocaleStringResource
                                     {
                                         LocaleResourceKey = resourceKey1,
                                         ResourceValue = "testValue1"
                                     };

            var resourceKey2 = new LocaleResourceKey
                                   {
                                       DateAdded = DateTime.UtcNow,
                                       Id = Guid.NewGuid(),
                                       Name = "testKey2",
                                       Notes = "test notes"
                                   };

            var resourceValue2 = new LocaleStringResource
            {
                LocaleResourceKey = resourceKey2,
                ResourceValue = "testValue2"
            };

            var language = new Language {Id = testGuid, LanguageCulture = "en-GB", Name = "TestLanguage"};

            _localizationRepositorySub.AllLanguageResources(testGuid).Returns(new List<LocaleStringResource> { resourceValue1, resourceValue2 });
            _localizationRepositorySub.Get(testGuid).Returns(language);

            var csv = _localizationService.ToCsv(language);

            Assert.AreEqual(csv, "testKey1,testValue1\r\ntestKey2,testValue2\r\n");
        }

        #endregion
    }
}
