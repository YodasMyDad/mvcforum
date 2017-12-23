namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.DomainModel;
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

        public static string GenerateSlug(string stringToSlug, IEnumerable<Entity> similarList, string previousSlug)
        {
            // url generator
            var slug = CreateUrl(stringToSlug);

            // To list the entities
            var matchingEntities = similarList.ToList();
                
            // if the similarList is empty, just return this slug
            if (!matchingEntities.Any())
            {
                return slug;
            }

            // If the previous slug is null, then it's a newly created Entity
            if (string.IsNullOrEmpty(previousSlug))
            {
                // Now check another entity doesn't have the same one
                if (matchingEntities.Any())
                {
                    // See if there is only one. And if it matches exactly
                    // someone else has this, grab all like it and do a count, stick a suffix on
                    slug = string.Concat(slug, "-", matchingEntities.Count);
                }
            }
            else
            {
                // It's an update, see if they have changed the title by checking slugs
                if (slug != previousSlug)
                {
                    // Name/Title has changed
                    if (matchingEntities.Any())
                    {
                        slug = string.Concat(slug, "-", matchingEntities.Count);
                    }
                }
            }

            return slug;
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