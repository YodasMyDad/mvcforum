using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    // Join Methods: http://msdn.microsoft.com/en-us/library/system.linq.enumerable.join.aspx
    // Fluent API: http://msdn.microsoft.com/en-us/library/hh295847%28v=vs.103%29.aspx

    public partial class LocalizationRepository : ILocalizationRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public LocalizationRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get all languages
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Language> GetAll()
        {
            return _context.Language.OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Get all the string resources for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<LocaleStringResource> GetAllValues(Guid languageId, int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey, strRes => strRes.LocaleResourceKey.Id, resKey => resKey.Id,
                      (strRes, resKey) => new { LocaleStringResource = strRes, LocaleResourceKey = resKey })
                .Count(joinResult => joinResult.LocaleStringResource.Language.Id == languageId);

            var results = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey, // The sequence to join to the first sequence.
                      strRes => strRes.LocaleResourceKey.Id, // A function to extract the join key from each element of the first sequence.
                        resKey => resKey.Id, // A function to extract the join key from each element of the second sequence
                        (strRes, resKey) => new { LocaleStringResource = strRes, LocaleResourceKey = resKey } // A function to create a result element from two matching elements.
                        )
                .Where(joinResult => joinResult.LocaleStringResource.Language.Id == languageId)
                .OrderBy(joinResult => joinResult.LocaleResourceKey.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.LocaleStringResource).ToList();

            return new PagedList<LocaleStringResource>(results, pageIndex, pageSize, totalCount);
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
        /// Get all resource values for all languages
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
        /// Return all the resource keys in the system - paged
        /// </summary>
        /// <returns></returns>
        public PagedList<LocaleResourceKey> GetAllResourceKeys(int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleResourceKey.Count();

            var results = _context.LocaleResourceKey
                .OrderBy(x => x.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item)
                .ToList();

            return new PagedList<LocaleResourceKey>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Return all the resource keys in the system
        /// </summary>
        /// <returns></returns>
        public IList<LocaleResourceKey> GetAllResourceKeys()
        {
            return _context.LocaleResourceKey.ToList();
        }

        /// <summary>
        /// Adds a new language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public Language Add(Language language)
        {
            _context.Language.Add(language);
            return language;
        }

        /// <summary>
        /// Get all the language resources for a language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public IList<LocaleStringResource> AllLanguageResources(Guid languageId)
        {
            return _context.LocaleStringResource
                .Where(x => x.Language.Id == languageId)
                .ToList();
        }

        /// <summary>
        /// Search through the resource values for a key
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search">The valkue</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<LocaleStringResource> SearchResourceValuesForKey(Guid languageId, string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey, strRes => strRes.LocaleResourceKey.Id, resKey => resKey.Id, (strRes, resKey) =>
                    new { LocaleStringResource = strRes, LocaleResourceKey = resKey }).Count(joinResult => joinResult.LocaleStringResource.Language.Id == languageId &&
                                                                                                                                                                                                                     joinResult.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()));

            var results = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey, // The sequence to join to the first sequence.
                        strRes => strRes.LocaleResourceKey.Id, // A function to extract the join key from each element of the first sequence.
                        resKey => resKey.Id, // A function to extract the join key from each element of the second sequence
                        (strRes, resKey) => new { LocaleStringResource = strRes, LocaleResourceKey = resKey } // A function to create a result element from two matching elements.
                    )
                .Where(joinResult => joinResult.LocaleStringResource.Language.Id == languageId &&
                    joinResult.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()))
                .OrderBy(joinResult => joinResult.LocaleResourceKey.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.LocaleStringResource).ToList();

            return new PagedList<LocaleStringResource>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Search through the string resource values for a matching string value
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="search">The resource value</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<LocaleStringResource> SearchResourceValues(Guid languageId, string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleStringResource.Count();
            var results = _context.LocaleStringResource
                .Include(x => x.Language)
                .Include(x => x.LocaleResourceKey)
                .Where(x => x.Language.Id == languageId)
                .Where(x => x.ResourceValue.ToUpper().Contains(search.ToUpper()))
                .OrderBy(x => x.ResourceValue)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item).ToList();

            return new PagedList<LocaleStringResource>(results, pageIndex, pageSize, totalCount);
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
        public PagedList<LocaleStringResource> SearchResourceKeys(Guid languageId, string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey,
                      strRes => strRes.LocaleResourceKey.Id,
                      resKey => resKey.Id,
                      (strRes, resKey) => new { LocaleStringResource = strRes, LocaleResourceKey = resKey })
                      .Count(joinResult => joinResult.LocaleStringResource.Language.Id == languageId
                          && joinResult.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()));

            var results = _context.LocaleStringResource
                .Join(_context.LocaleResourceKey, // The sequence to join to the first sequence.
                        strRes => strRes.LocaleResourceKey.Id, // A function to extract the join key from each element of the first sequence.
                        resKey => resKey.Id, // A function to extract the join key from each element of the second sequence
                        (strRes, resKey) => new { LocaleStringResource = strRes, LocaleResourceKey = resKey } // A function to create a result element from two matching elements.
                    )
                .Where(joinResult => joinResult.LocaleStringResource.Language.Id == languageId &&
                    joinResult.LocaleResourceKey.Name.ToUpper().Contains(search.ToUpper()))
                .OrderBy(joinResult => joinResult.LocaleResourceKey.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.LocaleStringResource).ToList();

            return new PagedList<LocaleStringResource>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Search through the resource keys
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<LocaleResourceKey> SearchResourceKeys(string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.LocaleResourceKey.Count();
            var results = _context.LocaleResourceKey
                            .Where(resKey => resKey.Name.ToUpper().Contains(search.ToUpper()))
                            .OrderBy(resKey => resKey.Name)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            return new PagedList<LocaleResourceKey>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Get a language using the language-culture string e.g. "en-GB"
        /// </summary>
        /// <param name="languageCulture"></param>
        /// <returns></returns>
        public Language GetLanguageByLanguageCulture(string languageCulture)
        {
            return _context.Language.FirstOrDefault(x => x.LanguageCulture == languageCulture);
        }

        /// <summary>
        /// Get a resource value using a key
        /// </summary>
        /// <param name="languageId"> </param>
        /// <param name="key"></param>
        /// <returns></returns>
        public LocaleStringResource GetResource(Guid languageId, string key)
        {
            // Get the language again - otherwise if the language was previously fetched then the lazy load of
            // resource members might fail with a session closed error...
            var lang = Get(languageId);

            return lang.LocaleStringResources.FirstOrDefault(localization => localization.LocaleResourceKey.Name == key);
        }

        /// <summary>
        /// Delete a resource key
        /// </summary>
        /// <param name="resourceKey"></param>
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

        /// <summary>
        /// Delete a resource value
        /// </summary>
        /// <param name="resourceValue"></param>
        public void DeleteResourceValue(LocaleStringResource resourceValue)
        {
            _context.LocaleStringResource.Remove(resourceValue);
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
        /// Get a resource key by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LocaleResourceKey GetResourceKey(string name)
        {
            return _context.LocaleResourceKey.FirstOrDefault(x => x.Name.Trim() == name);
        }

        /// <summary>
        /// Add a new resource key
        /// </summary>
        /// <param name="newLocaleResourceKey"></param>
        public LocaleResourceKey Add(LocaleResourceKey newLocaleResourceKey)
        {
            _context.LocaleResourceKey.Add(newLocaleResourceKey);
            return newLocaleResourceKey;
        }

        /// <summary>
        /// Add a new resource key
        /// </summary>
        /// <param name="newLocaleStringResource"></param>
        public LocaleStringResource Add(LocaleStringResource newLocaleStringResource)
        {
            _context.LocaleStringResource.Add(newLocaleStringResource);
            return newLocaleStringResource;
        }

        /// <summary>
        /// Get a language by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public Language Get(Guid id, bool removeTracking = false)
        {
            if (removeTracking)
            {
                return _context.Language.AsNoTracking().FirstOrDefault(lang => lang.Id == id);    
            }
            return _context.Language.FirstOrDefault(lang => lang.Id == id);
        }

        public void Delete(LocaleStringResource item)
        {
            _context.LocaleStringResource.Remove(item);
        }

        public void Delete(LocaleResourceKey item)
        {
            _context.LocaleResourceKey.Remove(item);
        }

        public void Update(LocaleStringResource item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.LocaleStringResource.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;
        }

        public void Update(LocaleResourceKey item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.LocaleResourceKey.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;
        }

        public void Update(Language item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Language.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Delete a language
        /// </summary>
        /// <param name="language"></param>
        public void Delete(Language language)
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
        }
    }
}
