using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace MVCForum.Utilities
{
    public static class EmoticonUtils
    {
        public static string EmoticonFolder
        {
            get { return VirtualPathUtility.ToAbsolute("~/content/images/emoticons/"); }
        }
        public static OrderedDictionary GetEmoticonHashTable()
        {
            var emoticons = new OrderedDictionary
            {
                {":D", "big-smile-emoticon-for-facebook.png"},
                {":O", "surprised-emoticon.png"},
                {":/", "unsure-emoticon.png"},  
                {":P", "facebook-tongue-out-emoticon.png"},
                {":)", "facebook-smiley-face-for-comments.png"},                
                {":(", "facebook-frown-emoticon.png"},
                {":'(", "facebook-cry-emoticon-crying-symbol.png"},
                {"O:)", "angel-emoticon.png"},
                {"3:)", "devil-emoticon.png"},              
                {"-_-", "squinting-emoticon.png"},
                {":*", "kiss-emoticon.png"},
                {"^_^", "kiki-emoticon.png"},                
                {":v", "pacman-emoticon.png"},
                {":3", "curly-lips-emoticon.png"},
                {"o.O", "confused-emoticon-wtf-symbol-for-facebook.png"},
                {";)", "wink-emoticon.png"},
                {"8-)", "glasses-emoticon.png"},
                {"8| B|", "sunglasses-emoticon.png"}
                //{">:O", "angry-emoticon.png"},
                //{">:(", "grumpy-emoticon.png"}
            };

            return emoticons;
        }

        public static string Emotify(string inputText)
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
    }
}
