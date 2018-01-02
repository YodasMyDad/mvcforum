namespace MvcForum.Core.ExtensionMethods
{
    using Models;
    using Models.Entities;

    public static class ExtendedDataExtensions
    {
        /// <summary>
        ///     Sets extended data on
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetExtendedDataValue<T>(this T entity, string key, string value)
            where T : ExtendedDataEntity
        {
            var extendedData = entity.ExtendedData;

            // Existing
            var foundExisting = false;

            // Check for existing key, and replace
            foreach (var edItem in extendedData)
            {
                // Does key match
                if (edItem.Key == key)
                {
                    // Found key
                    edItem.Value = value;

                    foundExisting = true;

                    // Kill Loop
                    break;
                }
            }

            // If we haven't found it, then add it
            if (!foundExisting)
            {
                extendedData.Add(new ExtendedDataItem {Key = key, Value = value});
            }

            // We have to reset the data to trigger the set
            entity.ExtendedData = extendedData;
        }

        /// <summary>
        ///     Removes an extended data item by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        public static void RemoveExtendedDataItem<T>(this T entity, string key)
            where T : ExtendedDataEntity
        {
            // Hold everything
            var extendedData = entity.ExtendedData;

            // Get the one to remove
            var toRemoveAt = 0;
            foreach (var extendedDataItem in entity.ExtendedData)
            {
                if (extendedDataItem.Key == key)
                {
                    break;
                }
                toRemoveAt++;
            }

            // Remove it
            extendedData.RemoveAt(toRemoveAt);

            // We have to reset the data to trigger the set
            entity.ExtendedData = extendedData;
        }

        /// <summary>
        ///     Gets an extended data value by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetExtendedDataItem<T>(this T entity, string key)
            where T : ExtendedDataEntity
        {
            foreach (var extendedDataItem in entity.ExtendedData)
            {
                if (extendedDataItem.Key == key)
                {
                    return extendedDataItem.Value;
                }
            }

            return string.Empty;
        }
    }
}