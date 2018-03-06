namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Constants;
    using Core;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Utilities;

    public partial class LocalizationService : ILocalizationService
    {
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _loggingService;
        private readonly ICacheService _cacheService;
        private Language _currentLanguage;
        private IMvcForumContext _context;
        private readonly Dictionary<string, string> _perRequestLanguageStrings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsService"> </param>
        /// <param name="loggingService"></param>
        /// <param name="cacheService"></param>
        /// <param name="context"></param>
        public LocalizationService(ISettingsService settingsService, ILoggingService loggingService, ICacheService cacheService, IMvcForumContext context)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;
            _cacheService = cacheService;
            _context = context;
            _perRequestLanguageStrings = ResourceKeysByLanguage(CurrentLanguage);
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _settingsService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        #region Sanitizing

        public Language SanitizeLanguage(Language language)
        {
            language.Name = StringUtils.SafePlainText(language.Name);
            language.LanguageCulture = StringUtils.SafePlainText(language.LanguageCulture);
            return language;
        }

        public LocaleResourceKey SanitizeLocaleResourceKey(LocaleResourceKey localeResourceKey)
        {
            localeResourceKey.Name = StringUtils.SafePlainText(localeResourceKey.Name);
            localeResourceKey.Notes = StringUtils.SafePlainText(localeResourceKey.Notes);
            return localeResourceKey;
        }

        public LocaleStringResource SanitizeLocaleStringResource(LocaleStringResource localeStringResource)
        {
            localeStringResource.ResourceValue = StringUtils.SafePlainText(localeStringResource.ResourceValue);
            return localeStringResource;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Update a resource string
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="resourceKey"></param>
        /// <param name="newValue"></param>
        public void UpdateResourceString(Guid languageId, string resourceKey, string newValue)
        {
            // Get the resource
            var localeStringResource = GetResource(languageId, resourceKey);

            if (localeStringResource == null)
            {
                throw new ApplicationException(
                    $"Unable to update resource with key {resourceKey} for language {languageId}. No resource found.");
            }
            localeStringResource.ResourceValue = StringUtils.SafePlainText(newValue);
            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
        }

        /// <summary>
        /// Add a new resource key
        /// </summary>
        /// <param name="newLocaleResourceKey"></param>
        public LocaleResourceKey Add(LocaleResourceKey newLocaleResourceKey)
        {

            // Trim name to stop any issues
            newLocaleResourceKey.Name = newLocaleResourceKey.Name.Trim();

            // Check to see if a respource key of this name already exists
            var existingResourceKey = GetResourceKey(newLocaleResourceKey.Name);

            if (existingResourceKey != null)
            {
                throw new ApplicationException(
                    $"The resource key with name '{newLocaleResourceKey.Name}' already exists.");
            }

            newLocaleResourceKey.DateAdded = DateTime.UtcNow;

            // Now add an empty value for each language
            newLocaleResourceKey.LocaleStringResources = new List<LocaleStringResource>();
            foreach (var language in GetAll())
            {
                var resourceValue = new LocaleStringResource
                {
                    Language = language,
                    LocaleResourceKey = newLocaleResourceKey,
                    ResourceValue = string.Empty
                };

                language.LocaleStringResources.Add(resourceValue);
            }

            //Sanitze
            newLocaleResourceKey = SanitizeLocaleResourceKey(newLocaleResourceKey);

            // Add the key
            var result = _context.LocaleResourceKey.Add(newLocaleResourceKey);

            // Clear hard cache for Languages
            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);

            return result;
        }

        /// <summary>
        /// Add a new language into the system (does NOT set current language)
        /// </summary>
        /// <param name="language"></param>
        public void Add(Language language)
        {
            // Does the language already exist by name or language-locale?
            var existingLanguage = GetLanguageByLanguageCulture(language.LanguageCulture);

            if (existingLanguage != null)
            {
                throw new LanguageOrCultureAlreadyExistsException(
                    $"There is already a language defined for language-culture '{existingLanguage.LanguageCulture}'");
            }

            // Make sure that the new language has a set of empty resources
            language.LocaleStringResources = new List<LocaleStringResource>();
            foreach (var localeResourceKey in GetAllResourceKeys())
            {
                var localeStringResource = new LocaleStringResource
                {
                    Language = language,
                    LocaleResourceKey = localeResourceKey,
                    ResourceValue = string.Empty
                };
                language.LocaleStringResources.Add(localeStringResource);
            }

            language = SanitizeLanguage(language);
            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
            _context.Language.Add(language);
        }

        /// <summary>
        /// Add a new language
        /// </summary>
        /// <param name="cultureInfo"></param>
        public Language Add(CultureInfo cultureInfo)
        {
            // Create a domain language object
            var language = new Language
            {
                Name = cultureInfo.EnglishName,
                LanguageCulture = cultureInfo.Name,
            };

            Add(language);
            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
            return language;
        }

        /// <summary>
        /// Get a resource value using a key
        /// </summary>
        /// <param name="languageId"> </param>
        /// <param name="key"></param>
        /// <returns></returns>
        public LocaleStringResource GetResource(Guid languageId, string key)
        {
            try
            {
                // Get the language again - otherwise if the language was previously fetched then the lazy load of
                // resource members might fail with a session closed error...
                var lang = Get(languageId);
                return lang.LocaleStringResources.FirstOrDefault(localization => localization.LocaleResourceKey.Name == key.Trim());
            }
            catch (Exception ex)
            {
                // Could be there is no resource
                _loggingService.Error(
                    $"Unable to retrieve resource key '{key}' for language id {languageId}. Error: '{ex.Message}'.");
                return null;
            }
        }

        /// <summary>
        /// Get a resource key by id
        /// </summary>
        /// <returns></returns>
        public LocaleStringResource GetResource(Language language, string key)
        {
            return GetResource(language.Id, key);
        }

        /// <summary>
        /// Get a resource key by id
        /// </summary>
        /// <returns></returns>
        public string GetResourceString(Language language, string key)
        {
            var resFormat = GetResource(language.Id, key);

            var resValue = resFormat?.ResourceValue;
            if (!string.IsNullOrWhiteSpace(resValue))
            {
                return new LocalizedString(resValue).Text;
            }
            return new LocalizedString(key).Text;
        }

        /// <summary>
        /// Get a resource key by id
        /// </summary>
        /// <returns></returns>
        public string GetResourceString(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                var trimmedKey = key.Trim();
                try
                {
                    if (_perRequestLanguageStrings.ContainsKey(trimmedKey))
                    {
                        var langValue = _perRequestLanguageStrings[trimmedKey];
                        if (!string.IsNullOrWhiteSpace(langValue))
                        {
                            return langValue;
                        }
                        _loggingService.Error(
                            $"No value is set for resource key '{trimmedKey}' for language {CurrentLanguage.Name}.");
                    }
                    else
                    {
                        _loggingService.Error(
                            $"This resource key '{trimmedKey}' was not found for the language {CurrentLanguage.Name}.");
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Error(ex);
                }
                return trimmedKey;
            }
            return string.Empty;
        }

        #endregion


        /// <summary>
        /// Update a resource key - change its name
        /// </summary>
        /// <param name="resourceKeyId"></param>
        /// <param name="newName"></param>
        public void UpdateResourceKey(Guid resourceKeyId, string newName)
        {

            // Get the resource
            var localeStringResourceKey = GetResourceKey(resourceKeyId);

            if (localeStringResourceKey == null)
            {
                throw new ApplicationException($"Unable to update resource key {resourceKeyId} . No resource found.");
            }

            localeStringResourceKey.Name = StringUtils.SafePlainText(newName);
            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
        }

        public async Task<PaginatedList<LocaleStringResource>> SearchResourceValuesForKey(Guid languageId, string search, int pageIndex, int pageSize)
        {
            var query = _context.LocaleStringResource.AsNoTracking()
                .Include(x => x.LocaleResourceKey)
                .Include(x => x.Language)
                .Where(x => x.Language.Id == languageId && x.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()))
                .OrderBy(x => x.LocaleResourceKey.Name);

            return await PaginatedList<LocaleStringResource>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public void DeleteResourceKey(LocaleResourceKey resourceKey)
        {
            // Delete all the string resources referencing this key
            var allValuesForKey = GetAllValuesForKey(resourceKey.Id);
            var allValuesToDelete = new List<LocaleStringResource>();
            allValuesToDelete.AddRange(allValuesForKey);
            foreach (var valueToDelete in allValuesToDelete)
            {
                _context.LocaleStringResource.Remove(valueToDelete);
            }

            _context.LocaleResourceKey.Remove(resourceKey);
        }

        public void DeleteResourceValue(LocaleStringResource resourceValue)
        {
            _context.LocaleStringResource.Remove(resourceValue);
        }

        public LocaleResourceKey GetResourceKey(string name)
        {
            return _context.LocaleResourceKey.FirstOrDefault(x => x.Name.Trim() == name);
        }

        public LocaleStringResource Add(LocaleStringResource newLocaleStringResource)
        {
            _context.LocaleStringResource.Add(newLocaleStringResource);
            return newLocaleStringResource;
        }

        public IList<LocaleStringResource> AllLanguageResources(Guid languageId)
        {
            return _context.LocaleStringResource
                        .Include(x => x.LocaleResourceKey)
                         .AsNoTracking()
                        .Where(x => x.Language.Id == languageId)
                        .ToList();
        }

        public void Delete(LocaleStringResource item)
        {
            _context.LocaleStringResource.Remove(item);
        }

        public void Delete(LocaleResourceKey item)
        {
            _context.LocaleResourceKey.Remove(item);
        }

        /// <summary>
        /// Get all the localization values (cached)
        /// </summary>
        /// <returns></returns>
        public Language CurrentLanguage
        {
            get
            {
                try
                {
                    if (HttpContext.Current != null)
                    {
                        // Check for cookie, as the user may have switched the language from the deafult one
                        var languageCooke = HttpContext.Current.Request.Cookies[Constants.LanguageIdCookieName];
                        if (languageCooke != null)
                        {
                            // See if it's the same language as already set
                            var languageGuid = new Guid(languageCooke.Value);
                            if (_currentLanguage != null && languageGuid == _currentLanguage.Id)
                            {
                                return _currentLanguage;
                            }

                            // User might have a language set
                            var changedLanguage = Get(languageGuid, true);
                            if (changedLanguage != null)
                            {
                                // User has set the language so overide it here
                                _currentLanguage = changedLanguage;
                            }
                        }
                    }

                }
                catch
                {
                    // App Start cause this to error
                    // http://stackoverflow.com/questions/2518057/request-is-not-available-in-this-context
                }

                return _currentLanguage ?? (_currentLanguage = DefaultLanguage);
            }

            set { _currentLanguage = value; }
        }

        /// <summary>
        /// The system default language
        /// </summary>
        public Language DefaultLanguage
        {
            get
            {
                var settings = _settingsService.GetSettings(false);

                if (settings == null)
                {
                    // This is a one off scenario and means the system has no settings
                    // usually when running the installer, so we need to return a default language
                    return new Language { Name = "Setup Language", LanguageCulture = "en-GB" };
                }

                // If we get here just set the default language
                var language = settings.DefaultLanguage;

                if (language == null)
                {
                    throw new ApplicationException("There is no default language defined in the system.");
                }

                return language;
            }
        }



        /// <summary>
        /// Retrieve a language by the language-culture string e.g. "en-GB"
        /// </summary>
        /// <param name="languageCulture"></param>
        public Language GetLanguageByLanguageCulture(string languageCulture)
        {
            return _context.Language.FirstOrDefault(x => x.LanguageCulture == languageCulture);
        }


        /// <summary>
        /// Retrieve a language by name
        /// </summary>
        /// <param name="name"></param>
        public Language GetLanguageByName(string name)
        {

            return GetLanguageByLanguageCulture(name);

        }

        /// <summary>
        /// All languages
        /// </summary>
        public IEnumerable<Language> AllLanguages => GetAll();

        /// <summary>
        /// Get paged set of resources for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<LocaleStringResource>> GetAllValues(Guid languageId, int pageIndex, int pageSize)
        {
            var results = _context.LocaleStringResource.AsNoTracking()
                .Include(x => x.Language)
                .Include(x => x.LocaleResourceKey)
                .Where(x => x.Language.Id == languageId)
                .OrderBy(x => x.LocaleResourceKey.Name);

            return await PaginatedList<LocaleStringResource>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get resource values for all languages for a key
        /// </summary>
        /// <param name="resourceKeyId"> </param>
        /// <returns></returns>
        public IList<LocaleStringResource> GetAllValuesForKey(Guid resourceKeyId)
        {
            return _context.LocaleStringResource
                        .Include(x => x.Language)
                        .Include(x => x.LocaleResourceKey)
                        .Where(strRes => strRes.LocaleResourceKey.Id == resourceKeyId)
                        .Select(strRes => strRes).ToList();
        }

        /// <summary>
        /// Get all resource keys - paged
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<LocaleResourceKey>> GetAllResourceKeys(int pageIndex, int pageSize)
        {
            var results = _context.LocaleResourceKey.AsNoTracking()
                .OrderBy(x => x.Name);

            return await PaginatedList<LocaleResourceKey>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get all resource strings for a language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public Dictionary<string, string> ResourceKeysByLanguage(Language language)
        {
            var cacheKey = string.Concat(Constants.LanguageStrings, language.Id);
            var cachedResourceKeys = _cacheService.Get<Dictionary<string, string>>(cacheKey);
            if (cachedResourceKeys == null)
            {
                cachedResourceKeys = GetAllLanguageStringsByLangauge(language.Id);
                _cacheService.Set(cacheKey, cachedResourceKeys, CacheTimes.OneDay);
            }
            return cachedResourceKeys;
        }

        /// <summary>
        /// Return all the resource keys in the system - non paged
        /// </summary>
        /// <returns></returns>
        public IList<LocaleResourceKey> GetAllResourceKeys()
        {
            return _context.LocaleResourceKey.ToList();
        }


        /// <summary>
        /// Search resources in a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<LocaleStringResource>> SearchResourceValues(Guid languageId, string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var results = _context.LocaleStringResource
                .Include(x => x.Language)
                .Include(x => x.LocaleResourceKey)
                .AsNoTracking()
                .Where(x => x.Language.Id == languageId)
                .Where(x => x.ResourceValue.ToUpper().Contains(search.ToUpper()))
                .OrderBy(x => x.ResourceValue);

            return await PaginatedList<LocaleStringResource>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }


        /// <summary>
        /// Search through the resource values
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<LocaleResourceKey>> SearchResourceKeys(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var results = _context.LocaleResourceKey.Include(x => x.LocaleStringResources).AsNoTracking()
                .Where(resKey => resKey.Name.ToUpper().Contains(search.ToUpper()))
                .OrderBy(resKey => resKey.Name);

            return await PaginatedList<LocaleResourceKey>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Search through the resource values for a language looking
        /// in a subset of keys denoted by the search term
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search">The resource value</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<LocaleStringResource>> SearchResourceKeys(Guid languageId, string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);

            var results = _context.LocaleStringResource
                .Include(x => x.Language)
                .Include(x => x.LocaleResourceKey)
                .Where(x => x.Language.Id == languageId &&
                            x.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()))
                .OrderBy(x => x.LocaleResourceKey.Name);

            return await PaginatedList<LocaleStringResource>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        public IList<CultureInfo> LanguagesAll
        {
            get
            {
                var allLanguagesNotInDb = new List<CultureInfo>();

                foreach (var cultureInfo in LanguageUtils.AllCultures)
                {
                    allLanguagesNotInDb.Add(cultureInfo);
                }

                return allLanguagesNotInDb.OrderBy(info => info.EnglishName).ToList();
            }
        }

        /// <summary>
        /// Get an individual language
        /// </summary>
        /// <param name="id"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public Language Get(Guid id, bool removeTracking = false)
        {
            if (removeTracking)
            {
                return _context.Language.Include(x => x.LocaleStringResources).AsNoTracking().FirstOrDefault(lang => lang.Id == id);
            }
            return _context.Language.Include(x => x.LocaleStringResources).FirstOrDefault(lang => lang.Id == id);
        }



        /// <summary>
        /// Delete a language
        /// </summary>
        /// <param name="language"></param>
        public void Delete(Language language)
        {
            // Cannot delete default language
            if (language.Id == DefaultLanguage.Id)
            {
                throw new ApplicationException("Deleting the default language is not allowed.");
            }

            try
            {
                var strResToDelete = new List<LocaleStringResource>();
                foreach (var localeStringRes in language.LocaleStringResources)
                {
                    strResToDelete.Add(localeStringRes);
                }
                foreach (var strToDelete in strResToDelete)
                {
                    _context.LocaleStringResource.Remove(strToDelete);
                }

                language.LocaleStringResources.Clear();
                _context.Language.Remove(language);
                _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"Unable to delete language: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a resource key - warning: this will delete all the associated resource strings in all languages
        /// for this key
        /// </summary>
        public void DeleteLocaleResourceKey(LocaleResourceKey resourceKey)
        {
            try
            {
                // Delete the key and its values
                DeleteResourceKey(resourceKey);
                _cacheService.ClearStartsWith(Constants.LocalisationCacheName);

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Unable to delete resource key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get a resource key by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LocaleResourceKey GetResourceKey(Guid id)
        {
            return _context.LocaleResourceKey.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Creates a new, unsaved resource key, with default (empty) values
        /// </summary>
        /// <returns></returns>
        public LocaleResourceKey CreateEmptyLocaleResourceKey()
        {
            return new LocaleResourceKey
            {
                LocaleStringResources = new List<LocaleStringResource>(),
                Name = string.Empty,
                Notes = string.Empty,
                DateAdded = (DateTime)SqlDateTime.MinValue,
            };
        }

        /// <summary>
        /// Convert a language into CSV format (e.g. for export)
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public string ToCsv(Language language)
        {
            var csv = new StringBuilder();

            foreach (var resource in AllLanguageResources(language.Id))
            {
                csv.AppendFormat("{0},{1}", resource.LocaleResourceKey.Name, resource.ResourceValue);
                csv.AppendLine();
            }

            return csv.ToString();
        }


        public CsvReport FromCsv(Language language, List<string> allLines)
        {
            var commaSeparator = new[] { ',' };
            var report = new CsvReport();

            if (allLines == null || allLines.Count == 0)
            {
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                    Message = "No language keys or values found."
                });
                return report;
            }

            try
            {
                //var allResourceKeys = GetAllResourceKeys();
                var lineCounter = 0;
                foreach (var line in allLines)
                {
                    lineCounter++;

                    //var keyValuePair = line.Split(commaSeparator);
                    var keyValuePair = line.Split(commaSeparator, 2, StringSplitOptions.None);

                    if (keyValuePair.Length < 2)
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = $"Line {lineCounter}: a key and a value are required."
                        });

                        continue;
                    }

                    var key = keyValuePair[0];

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        // Ignore empty keys
                        continue;
                    }
                    key = key.Trim();

                    var value = keyValuePair[1];

                    var resourceKey = GetResourceKey(key);

                    if (language == null)
                    {
                        throw new ApplicationException("Unable to create language");
                    }

                    // If key does not exist, it is a new one to be created
                    if (resourceKey == null)
                    {
                        resourceKey = new LocaleResourceKey
                        {
                            Name = key,
                            DateAdded = DateTime.UtcNow,
                        };

                        Add(resourceKey);
                        report.Warnings.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.NewKeyCreated,
                            Message =
                                $"A new key named '{key}' has been created, and will require a value in all languages."
                        });
                    }

                    // In the new language (only) set the value for the resource
                    var stringResource = language.LocaleStringResources.FirstOrDefault(res => res.LocaleResourceKey.Name == resourceKey.Name);
                    if (stringResource != null)
                    {
                        if (!stringResource.ResourceValue.Equals(value))
                        {
                            stringResource.ResourceValue = value;   
                        }                     
                    }
                    else
                    {
                        // No string resources have been created, so most probably
                        // this is the installer creating the keys so we need to create the 
                        // string resource to go with it and add it
                        stringResource = new LocaleStringResource
                        {
                            Language = language,
                            LocaleResourceKey = resourceKey,
                            ResourceValue = value
                        };
                        Add(stringResource);
                    }
                }
            }
            catch (Exception ex)
            {
                report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.GeneralError, Message = ex.Message });
            }

            _cacheService.ClearStartsWith(Constants.LocalisationCacheName);
            return report;
        }

        public IEnumerable<Language> GetAll()
        {
            return _context.Language.OrderBy(x => x.Name).ToList();
        }

        public Dictionary<string, string> GetAllLanguageStringsByLangauge(Guid languageId)
        {
            var results = _context.LocaleStringResource
                    .Include(x => x.Language)
                    .Include(x => x.LocaleResourceKey)
                    .Where(x => x.Language.Id == languageId)
                      .Join(_context.LocaleResourceKey,
                            strRes => strRes.LocaleResourceKey.Id,
                            resKey => resKey.Id,
                            (strRes, resKey) =>
                            new { LocaleStringResource = strRes, LocaleResourceKey = resKey })
                      .ToDictionary(arg => arg.LocaleResourceKey.Name.Trim(), arg => arg.LocaleStringResource.ResourceValue);

            return results;
        }

        /// <summary>
        /// Import a language from CSV
        /// </summary>
        /// <param name="langKey"> </param>
        /// <param name="allLines"></param>
        /// <returns>A report on the import</returns>
        public CsvReport FromCsv(string langKey, List<string> allLines)
        {
            var report = new CsvReport();

            if (allLines == null || allLines.Count == 0)
            {
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                    Message = "No language keys or values found."
                });
                return report;
            }

            // Look up the language and culture
            Language language;

            try
            {
                var cultureInfo = LanguageUtils.GetCulture(langKey);

                if (cultureInfo == null)
                {
                    report.Errors.Add(new CsvErrorWarning
                    {
                        ErrorWarningType = CsvErrorWarningType.DoesNotExist,
                        Message = $"The language culture '{langKey}' does not exist."
                    });

                    return report;
                }

                // See if this language exists already, and if not then create it
                language = GetLanguageByLanguageCulture(langKey) ?? Add(cultureInfo);
            }
            catch (LanguageOrCultureAlreadyExistsException ex)
            {
                report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.AlreadyExists, Message = ex.Message });
                return report;
            }
            catch (Exception ex)
            {
                report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.ItemBad, Message = ex.Message });
                return report;
            }

            return FromCsv(language, allLines);
        }

        /// <summary>
        /// Finds all languages in database and returns them as culture info objects
        /// </summary>
        public IList<CultureInfo> LanguagesInDb
        {
            get
            {
                return AllLanguages.Select(language => LanguageUtils.GetCulture(language.LanguageCulture)).OrderBy(info => info.EnglishName).ToList();
            }
        }

        /// <summary>
        /// Finds all languages NOT in database and returns them as culture info objects
        /// </summary>
        public IList<CultureInfo> LanguagesNotInDb
        {
            get
            {
                var allLanguagesNotInDb = new List<CultureInfo>();
                var allLanguagesInDb = AllLanguages.ToList();

                foreach (var cultureInfo in LanguageUtils.AllCultures)
                {
                    var found = allLanguagesInDb.Any(language => language.LanguageCulture == cultureInfo.Name);
                    if (!found)
                    {
                        allLanguagesNotInDb.Add(cultureInfo);
                    }

                }

                return allLanguagesNotInDb.OrderBy(info => info.EnglishName).ToList();
            }
        }


    }

    public class LanguageOrCultureAlreadyExistsException : Exception
    {
        public LanguageOrCultureAlreadyExistsException(string message)
            : base(message)
        {

        }
    }
}

