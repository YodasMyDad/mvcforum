namespace MvcForum.Core.Interfaces.Services
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    ///     This class deals with and supplies all config based data
    /// </summary>
    public partial interface IConfigService
    {
        #region Emojies

        string Emotify(string inputText);
        OrderedDictionary GetEmoticonHashTable();
        Dictionary<string, string> GetForumConfig();
        Dictionary<string, string> GetPlugins();

        #endregion
    }
}