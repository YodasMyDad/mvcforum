using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MVCForum.Utilities
{
    public static class LanguageUtils
    {
        private static SortedDictionary<string, CultureInfo> _allCultures;

        /// <summary>
        /// Read and store all the cultures
        /// </summary>
        private static void ReadCultures()
        {
            if (_allCultures == null)
            {
                _allCultures = new SortedDictionary<string, CultureInfo>();

                // Store and sort by English name
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList())
                {
                    if (!_allCultures.ContainsKey(culture.Name))
                    {
                        _allCultures.Add(culture.Name, culture);
                    }
                }
            }
        }

        /// <summary>
        /// Get all cultures sorted by name
        /// </summary>
        public static IEnumerable<CultureInfo> AllCultures
        {
            get
            {
                ReadCultures();
                return _allCultures.Values.OrderBy(info => info.EnglishName);
            }
        }

        /// <summary>
        /// True if the collection contains a culture according to a language-culture string
        /// </summary>
        /// <param name="nameKey"></param>
        /// <returns></returns>
        public static bool Contains(string nameKey)
        {
            return _allCultures.ContainsKey(nameKey);
        }

        public static int Count
        {
            get { return _allCultures.Count; }
        }


        /// <summary>
        /// Retrieve a specific culture by name
        /// </summary>
        /// <param name="nameKey"></param>
        /// <returns>The culture info for the key, or null if not found</returns>
        public static CultureInfo GetCulture(string nameKey)
        {
            ReadCultures();

            return _allCultures.ContainsKey(nameKey) ? _allCultures[nameKey] : null;
        }
    }
}
