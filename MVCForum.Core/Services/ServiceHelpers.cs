namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities;

    public static class ServiceHelpers
    {        
        #region Cache

        /// <summary>
        /// permission cache key for getting and setting keys
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static string ReturnCacheKey(Guid categoryId, Guid roleId)
        {
            return $"permissioncache-{categoryId}-{roleId}";
        }
        #endregion

        #region Url

        /// <summary>
        /// Creates a friendly url
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateUrl(string name)
        {
            return StringUtils.CreateUrl(name, "-");
        }

        public static string GenerateSlug(string stringToSlug, List<string> similarSlugs, string previousSlug)
        {
            // url generator
            var slug = CreateUrl(stringToSlug);
                
            // if the similarList is empty, just return this slug
            if (!similarSlugs.Any())
            {
                return slug;
            }

            // If the previous slug is null, then it's a newly created Entity
            if (string.IsNullOrWhiteSpace(previousSlug))
            {
                // Create the slug
                slug = MakeSafeSlug(similarSlugs, slug);
            }
            else
            {
                // It's an update, see if they have changed the title by checking slugs
                if (slug != previousSlug)
                {
                    // Name/Title has changed
                    slug = MakeSafeSlug(similarSlugs, slug);
                }
            }

            return slug;
        }

        private static string MakeSafeSlug(IReadOnlyCollection<string> similarSlugs, string currentSlug)
        {
            // Firstly check to see if the slug matches any of the existing
            var slugIsUnique = true;
            foreach (var slug in similarSlugs)
            {
                if (currentSlug.ToLower() == slug)
                {
                    slugIsUnique = false;
                    break;
                }
            }

            // If this is false then one of the slugs is the same, so append the count
            if (slugIsUnique == false)
            {
                var updatedSlug = $"{currentSlug}-{similarSlugs.Count}";
                currentSlug = MakeSafeSlug(similarSlugs, updatedSlug);
            }

            return currentSlug;
        }

        #endregion

        #region Extension Methods

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        #endregion
    }
}