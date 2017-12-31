namespace MvcForum.Core.Models.Activity
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The purpose of this class is to add a base class to our more-derived activity classes,
    /// such as BadgeActivity. Those classes cannot inherit straight from the domain Activity class
    /// because NHibernate does not support inheritance. It will not recognise the derived classes as
    /// being instances of the mapped classes, so e.g. they cannot be directly saved. 
    /// This is obvious because NHibernate makes proxies.
    /// </summary>
    public abstract class ActivityBase
    {
        public const string Equality = @"=";
        protected const string Separator = @",";
        protected const string RegexNameValue = @"^([^=]+)=([^=]+)$";

        public Activity ActivityMapped { get; set; }

        /// <summary>
        /// Turn the unprocessed data into keyed name-value pairs
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> UnpackData(Activity activity)
        {
            if (activity == null)
            {
                throw new ApplicationException("Attempting to unpack activity data when no database record.");
            }

            var keyValuePairs = new Dictionary<string, string>();

            // Form of data is "name=value,name=value" etc
            var keyValuePairsRaw = activity.Data.Split(new[] { ',' });

            var pattern = new Regex(RegexNameValue, RegexOptions.None);

            foreach (var keyValuePairRaw in keyValuePairsRaw)
            {
                var match = pattern.Match(keyValuePairRaw);

                if (match.Success)
                {
                    keyValuePairs.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
            }

            return keyValuePairs;
        }
    }
}
