using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Utilities;

namespace MVCForum.Services
{

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
            return string.Format("permissioncache-{0}-{1}", categoryId, roleId);
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


        /// <summary>
        /// Turn a name into a slug
        /// </summary>
        /// <typeparam name="T">The entity type eg MembershipUser</typeparam>
        /// <param name="arg">The key to use ie the name to convert</param>
        /// <param name="predicate">The method used to get the entitites that have a similar slug 
        /// by e.g. _membershipRepository.GetUserBySlugLike(userToImport.UserName) </param>
        /// <returns>A usable unique slug</returns>
        public static string GenerateSlug<T>(string arg, Func<string, IList<T>> predicate)
        {
            // url generator
            var slug = CreateUrl(arg);

            // Now check another entity doesn't have the same one
            var usersBySlug = predicate(arg);
            if (usersBySlug.Any())
            {
                // someone else has this, grab all like it and do a count, stick a suffix on
                slug = string.Concat(slug, "-", usersBySlug.Count());
            }

            return slug;
        }

        #endregion

        #region Extension Methods

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        #endregion
    }
}