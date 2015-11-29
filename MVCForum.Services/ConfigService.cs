using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class ConfigService : IConfigService
    {
        private static string EmoticonFolder => VirtualPathUtility.ToAbsolute("~/content/images/emoticons/");

        private readonly CacheService _cacheService;
        public ConfigService(CacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public string Emotify(string inputText)
        {
            var emoticonFolder = EmoticonFolder;
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
                var webConfigPath = HostingEnvironment.MapPath("~/config/emoticons.config");
                if (webConfigPath != null)
                {
                    var xDoc = new XmlDocument();
                    xDoc.Load(webConfigPath);
                    XmlNode root = xDoc.DocumentElement;
                    if (root != null)
                    {
                        var emoticonNodes = root.SelectNodes("//emoticon");
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

                            _cacheService.Set(key, emoticons, CacheTimes.TwelveHours);
                        }
                    }
                }
            }

            return emoticons;
        }
    }
}
