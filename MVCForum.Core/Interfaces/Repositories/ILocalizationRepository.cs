using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface ILocalizationRepository
    {
        IEnumerable<Language> GetAll();

        /// <summary>
        /// Get a language using the language-culture string e.g. "en-GB"
        /// </summary>
        /// <param name="languageCulture"></param>
        /// <returns></returns>
        Language GetLanguageByLanguageCulture(string languageCulture);

        /// <summary>
        /// Get a resource value from a language using a key
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        LocaleStringResource GetResource(Guid languageId, string key);

        /// <summary>
        /// Search through the resource values for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<LocaleStringResource> SearchResourceValues(Guid languageId, string search, int pageIndex, int pageSize);

        /// <summary>
        /// Search through the resource values for a language looking
        /// in a subset of keys denoted by the search term
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search">The resource value</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<LocaleStringResource> SearchResourceKeys(Guid languageId, string search, int pageIndex, int pageSize);

        PagedList<LocaleStringResource> SearchResourceValuesForKey(Guid languageId, string search, int pageIndex, int pageSize);

        /// <summary>
        /// Search through the resource values
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<LocaleResourceKey> SearchResourceKeys(string search, int pageIndex, int pageSize);

        /// <summary>
        /// Get all resource values for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<LocaleStringResource> GetAllValues(Guid languageId, int pageIndex, int pageSize);

        /// <summary>
        /// Get all the values with keys for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Dictionary<string, string> GetAllLanguageStringsByLangauge(Guid languageId);

        /// <summary>
        /// Get resource values for all languages for a key
        /// </summary>
        /// <param name="resourceKeyId"> </param>
        /// <returns></returns>
        IList<LocaleStringResource> GetAllValuesForKey(Guid resourceKeyId);

        /// <summary>
        /// Return all the resource keys in the system - paged
        /// </summary>
        /// <returns></returns>
        PagedList<LocaleResourceKey> GetAllResourceKeys(int pageIndex, int pageSize);

        /// <summary>
        /// Return all the resource keys in the system - non paged
        /// </summary>
        /// <returns></returns>
        IList<LocaleResourceKey> GetAllResourceKeys();

        /// <summary>
        /// Delete a language
        /// </summary>
        /// <param name="language"></param>
        void Delete(Language language);

        /// <summary>
        /// Delete a resource key
        /// </summary>
        /// <param name="resourceKey"></param>
        void DeleteResourceKey(LocaleResourceKey resourceKey);

        /// <summary>
        /// Delete a resource value
        /// </summary>
        /// <param name="resourceValue"></param>
        void DeleteResourceValue(LocaleStringResource resourceValue);

        /// <summary>
        /// Get a resource key by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LocaleResourceKey GetResourceKey(Guid id);

        /// <summary>
        /// Get a resource key by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        LocaleResourceKey GetResourceKey(string name);
       
        /// <summary>
        /// Add a new resource key
        /// </summary>
        /// <param name="newLocaleResourceKey"></param>
        LocaleResourceKey Add(LocaleResourceKey newLocaleResourceKey);

        /// <summary>
        /// Add a new resource key
        /// </summary>
        /// <param name="newLocaleStringResource"></param>
        LocaleStringResource Add(LocaleStringResource newLocaleStringResource);

        /// <summary>
        /// Adds a new language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        Language Add(Language language);

        /// <summary>
        /// Get all the language resources for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        IList<LocaleStringResource> AllLanguageResources(Guid languageId);


        /// <summary>
        /// Get a language by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Language Get(Guid id);

        void Delete(LocaleStringResource item);
        void Delete(LocaleResourceKey item);

        void Update(LocaleStringResource item);
        void Update(LocaleResourceKey item);
        void Update(Language item);

    }
}
