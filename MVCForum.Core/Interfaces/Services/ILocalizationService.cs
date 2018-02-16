namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.General;

    public interface ILocalizationService : IContextService
    {
        Language SanitizeLanguage(Language language);
        LocaleResourceKey SanitizeLocaleResourceKey(LocaleResourceKey localeResourceKey);
        LocaleStringResource SanitizeLocaleStringResource(LocaleStringResource localeStringResource);

        #region Language

        /// <summary>
        ///     Finds all languages in database and returns them as culture info objects
        /// </summary>
        IList<CultureInfo> LanguagesInDb { get; }

        /// <summary>
        ///     Finds all languages NOT in database and returns them as culture info objects
        /// </summary>
        IList<CultureInfo> LanguagesNotInDb { get; }

        /// <summary>
        ///     Returns all languages and returns them as culture info objects
        /// </summary>
        IList<CultureInfo> LanguagesAll { get; }

        /// <summary>
        ///     Get a language by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        Language Get(Guid id, bool removeTracking = false);

        /// <summary>
        ///     Get all languages
        /// </summary>
        IEnumerable<Language> AllLanguages { get; }

        /// <summary>
        ///     Retrieve a language by the language-culture string e.g. "en-GB"
        /// </summary>
        /// <param name="languageCulture"></param>
        Language GetLanguageByLanguageCulture(string languageCulture);

        /// <summary>
        ///     Retrieve a language by name
        /// </summary>
        /// <param name="name"></param>
        Language GetLanguageByName(string name);

        /// <summary>
        ///     The system default language
        /// </summary>
        Language DefaultLanguage { get; }

        /// <summary>
        ///     Current language in use
        /// </summary>
        Language CurrentLanguage { get; set; }

        /// <summary>
        ///     Delete a resource key
        /// </summary>
        /// <param name="resourceKey"></param>
        void DeleteLocaleResourceKey(LocaleResourceKey resourceKey);

        /// <summary>
        ///     Add a new language
        /// </summary>
        /// <param name="language"></param>
        void Add(Language language);

        /// <summary>
        ///     Add a new language
        /// </summary>
        /// <param name="cultureInfo"></param>
        Language Add(CultureInfo cultureInfo);

        /// <summary>
        ///     Delete a language
        /// </summary>
        /// <param name="language"></param>
        void Delete(Language language);

        /// <summary>
        ///     Convert a language into CSV format (e.g. for export)
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        string ToCsv(Language language);

        /// <summary>
        ///     Import a language from CSV
        /// </summary>
        /// <param name="langKey"> </param>
        /// <param name="allLines"></param>
        /// <returns>A report on the import</returns>
        CsvReport FromCsv(string langKey, List<string> allLines);

        CsvReport FromCsv(Language lang, List<string> allLines);

        IEnumerable<Language> GetAll();

        /// <summary>
        ///     Get all the values with keys for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Dictionary<string, string> GetAllLanguageStringsByLangauge(Guid languageId);

        #endregion

        #region Resource

        /// <summary>
        ///     Search resources for value - paged
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedList<LocaleStringResource>> SearchResourceValues(Guid languageId, string search, int pageIndex,
            int pageSize);

        /// <summary>
        ///     Search through the resource values
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedList<LocaleResourceKey>> SearchResourceKeys(string search, int pageIndex, int pageSize);

        /// <summary>
        ///     Search through the resource values for a language looking
        ///     in a subset of keys denoted by the search term
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search">The resource value</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedList<LocaleStringResource>> SearchResourceKeys(Guid languageId, string search, int pageIndex, int pageSize);

        /// <summary>
        ///     Get all resource values for a language - paged
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedList<LocaleStringResource>> GetAllValues(Guid languageId, int pageIndex, int pageSize);

        /// <summary>
        ///     Get resource values for all languages for a key
        /// </summary>
        /// <param name="resourceKeyId"> </param>
        /// <returns></returns>
        IList<LocaleStringResource> GetAllValuesForKey(Guid resourceKeyId);

        /// <summary>
        ///     Get all resource keys - paged
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedList<LocaleResourceKey>> GetAllResourceKeys(int pageIndex, int pageSize);

        /// <summary>
        ///     Puts the entire
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        Dictionary<string, string> ResourceKeysByLanguage(Language language);

        /// <summary>
        ///     Return all the resource keys in the system - non paged
        /// </summary>
        /// <returns></returns>
        IList<LocaleResourceKey> GetAllResourceKeys();

        /// <summary>
        ///     Gets the language text for the current language based on the key
        /// </summary>
        /// <param name="language"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetResourceString(Language language, string key);

        /// <summary>
        ///     Gets the language text for the current language based on the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetResourceString(string key);

        /// <summary>
        ///     Get a resource by key
        /// </summary>
        /// <param name="language"> </param>
        /// <param name="key"></param>
        /// <returns></returns>
        LocaleStringResource GetResource(Language language, string key);

        /// <summary>
        ///     Get a resource by key
        /// </summary>
        /// <param name="languageId"> </param>
        /// <param name="key"></param>
        /// <returns></returns>
        LocaleStringResource GetResource(Guid languageId, string key);


        /// <summary>
        ///     Get a resource key by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LocaleResourceKey GetResourceKey(Guid id);

        /// <summary>
        ///     Creates a new, unsaved resource key, with default (empty) values
        /// </summary>
        /// <returns></returns>
        LocaleResourceKey CreateEmptyLocaleResourceKey();

        /// <summary>
        ///     Add a new resource key
        /// </summary>
        /// <param name="newLocaleResourceKey"></param>
        LocaleResourceKey Add(LocaleResourceKey newLocaleResourceKey);

        /// <summary>
        ///     Update a resource string
        /// </summary>
        /// <param name="language"></param>
        /// <param name="resourceKey"></param>
        /// <param name="newValue"></param>
        void UpdateResourceString(Guid language, string resourceKey, string newValue);

        /// <summary>
        ///     Update a resource key - change its name
        /// </summary>
        /// <param name="resourceKeyId"></param>
        /// <param name="newName"></param>
        void UpdateResourceKey(Guid resourceKeyId, string newName);

        Task<PaginatedList<LocaleStringResource>> SearchResourceValuesForKey(Guid languageId, string search, int pageIndex,
            int pageSize);

        /// <summary>
        ///     Delete a resource key
        /// </summary>
        /// <param name="resourceKey"></param>
        void DeleteResourceKey(LocaleResourceKey resourceKey);

        /// <summary>
        ///     Delete a resource value
        /// </summary>
        /// <param name="resourceValue"></param>
        void DeleteResourceValue(LocaleStringResource resourceValue);

        /// <summary>
        ///     Get a resource key by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        LocaleResourceKey GetResourceKey(string name);

        /// <summary>
        ///     Add a new resource key
        /// </summary>
        /// <param name="newLocaleStringResource"></param>
        LocaleStringResource Add(LocaleStringResource newLocaleStringResource);

        /// <summary>
        ///     Get all the language resources for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        IList<LocaleStringResource> AllLanguageResources(Guid languageId);

        void Delete(LocaleStringResource item);
        void Delete(LocaleResourceKey item);

        #endregion
    }
}