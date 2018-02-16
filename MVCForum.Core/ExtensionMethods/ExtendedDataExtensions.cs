namespace MvcForum.Core.ExtensionMethods
{
    using Models;
    using Models.Entities;
    using Newtonsoft.Json;

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
        /// Sets extended data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TTwo"></typeparam>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetExtendedDataValue<T, TTwo>(this T entity, string key, TTwo value)
            where T : ExtendedDataEntity
        {
            // Converted value
            var convertedValue = JsonConvert.SerializeObject(value);

            entity.SetExtendedDataValue(key, convertedValue);
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
            if (entity.ExtendedData.Count > 0)
            {
                // Hold everything
                var extendedData = entity.ExtendedData;

                // Get the one to remove
                var toRemoveAt = 0;
                for (var index = 0; index < entity.ExtendedData.Count; index++)
                {
                    toRemoveAt = index;
                    var extendedDataItem = entity.ExtendedData[index];
                    if (extendedDataItem.Key == key)
                    {
                        break;
                    }
                }

                // Remove it
                extendedData.RemoveAt(toRemoveAt);

                // We have to reset the data to trigger the set
                entity.ExtendedData = extendedData;
            }
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

        /// <summary>
        /// Gets an extended data item and convert it to a type
        /// </summary>
        /// <typeparam name="TOne"></typeparam>
        /// <typeparam name="TTwo"></typeparam>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TTwo GetExtendedDataItem<TOne, TTwo>(this TOne entity, string key)
            where TOne : ExtendedDataEntity
        {
            foreach (var extendedDataItem in entity.ExtendedData)
            {
                if (extendedDataItem.Key == key)
                {
                    return JsonConvert.DeserializeObject<TTwo>(extendedDataItem.Value);
                }
            }

            return default(TTwo);
        }

    }


}