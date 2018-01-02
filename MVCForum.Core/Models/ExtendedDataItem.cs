namespace MvcForum.Core.Models
{
    /// <summary>
    ///     A serialisable class that is used to store extra data on entities
    /// </summary>
    public class ExtendedDataItem
    {
        /// <summary>
        ///     The key of the data
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     The value
        /// </summary>
        public string Value { get; set; }
    }
}