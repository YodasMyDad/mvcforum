using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Xml;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class ConfigService : IConfigService
    {
        private readonly ICacheService _cacheService;
        public ConfigService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        private static string EmoticonImageFolder => VirtualPathUtility.ToAbsolute("~/content/images/emoticons/");

        #region Emoticons

        public string Emotify(string inputText)
        {
            var emoticonFolder = EmoticonImageFolder;
            var emoticons = GetEmoticonHashTable();

            var sb = new StringBuilder(inputText.Length);

            for (var i = 0; i < inputText.Length; i++)
            {
                var strEmote = string.Empty;
                foreach (string emote in emoticons.Keys)
                {
                    if (inputText.Length - i >= emote.Length && emote.Equals(inputText.Substring(i, emote.Length), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Do custom checks in subStringExtraValue to stop emoticons replacing Html
                        var startIndex = (i >= 3 ? 3 : i);
                        var length = (startIndex * 2);
                        var subStringExtraValue = inputText.Substring(i - startIndex, emote.Length + length);

                        // Not brilliant, but for now will stop most cases
                        if (!subStringExtraValue.Contains("//"))
                        {
                            strEmote = emote;
                            break;
                        }
                    }
                }

                if (strEmote.Length != 0)
                {
                    sb.AppendFormat("<img src=\"{0}{1}\" alt=\"\" class=\"emoticon\" />", emoticonFolder, emoticons[strEmote]);
                    i += strEmote.Length - 1;
                }
                else
                {
                    sb.Append(inputText[i]);
                }
            }
            return sb.ToString();
        }

        public OrderedDictionary GetEmoticonHashTable()
        {
            const string key = "GetEmoticonHashTable";
            var emoticons = _cacheService.Get<OrderedDictionary>(key);
            if (emoticons == null)
            {
                emoticons = new OrderedDictionary();
                var root = SiteConfig.Instance.GetSiteConfig();
                    var emoticonNodes = root?.SelectNodes("/forum/emoticons/emoticon");
                    if (emoticonNodes != null)
                    {
                        foreach (XmlNode emoticonNode in emoticonNodes)
                        {
                            //<emoticon symbol="O:)" image="angel-emoticon.png" />  
                            if (emoticonNode.Attributes != null)
                            {
                                var emoticonSymbolAttr = emoticonNode.Attributes["symbol"];
                                var emoticonImageAttr = emoticonNode.Attributes["image"];
                                if (emoticonSymbolAttr != null && emoticonImageAttr != null)
                                {
                                    emoticons.Add(emoticonSymbolAttr.InnerText, emoticonImageAttr.InnerText);
                                }
                            }
                        }

                        _cacheService.Set(key, emoticons, CacheTimes.OneDay);
                    }
              
            }

            return emoticons;
        }
        #endregion

        #region Settings

        public Dictionary<string, string> GetForumConfig()
        {
            const string key = "SiteConfig";
            var siteConfig = _cacheService.Get<Dictionary<string, string>>(key);
            if (siteConfig == null)
            {
                siteConfig = new Dictionary<string, string>();
                var root = SiteConfig.Instance.GetSiteConfig();
                    var nodes = root?.SelectNodes("/forum/settings/setting");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            //<emoticon symbol="O:)" image="angel-emoticon.png" />  
                            if (node.Attributes != null)
                            {
                                var keyAttr = node.Attributes["key"];
                                var valueAttr = node.Attributes["value"];
                                if (keyAttr != null && valueAttr != null)
                                {
                                    siteConfig.Add(keyAttr.InnerText, valueAttr.InnerText);
                                }
                            }
                        }

                        _cacheService.Set(key, siteConfig, CacheTimes.OneDay);
                    }
             
            }
            return siteConfig;
        }

        #endregion

        #region Types

        public Dictionary<string, string> GetTypes()
        {
            const string key = "SiteTypes";
            var siteConfig = _cacheService.Get<Dictionary<string, string>>(key);
            if (siteConfig == null)
            {
                siteConfig = new Dictionary<string, string>();
                var root = SiteConfig.Instance.GetSiteConfig();
                var nodes = root?.SelectNodes("/forum/types/type");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        //<emoticon symbol="O:)" image="angel-emoticon.png" />  
                        if (node.Attributes != null)
                        {
                            var keyAttr = node.Attributes["name"];
                            var valueAttr = node.Attributes["value"];
                            if (keyAttr != null && valueAttr != null)
                            {
                                siteConfig.Add(keyAttr.InnerText, valueAttr.InnerText);
                            }
                        }
                    }

                    _cacheService.Set(key, siteConfig, CacheTimes.OneDay);
                }

            }
            return siteConfig;
        }

        #endregion

    }
}
