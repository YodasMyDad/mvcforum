using System;
using System.Collections;
using System.Text;
using System.Web;

namespace MVCForum.Utilities
{
    public static class EmoticonUtils
    {        
        public static Hashtable GetEmoticonHashTable()
        {
            var emoticons = new Hashtable(100)
            {
                {":)", "facebook-smiley-face-for-comments.png"},
                {":D", "big-smile-emoticon-for-facebook.png"},
                {":(", "facebook-frown-emoticon.png"},
                {":'(", "facebook-cry-emoticon-crying-symbol.png"},
                {":P", "facebook-tongue-out-emoticon.png"},
                {"O:)", "angel-emoticon.png"},
                {"3:)", "devil-emoticon.png"},
            };
            
            return emoticons;
        }

        public static string Emotify(string inputText)
        {
            var emoticonFolder = VirtualPathUtility.ToAbsolute("~/content/images/emoticons/");
            var emoticons = GetEmoticonHashTable();

            var sb = new StringBuilder(inputText.Length);

            for (var i = 0; i < inputText.Length; i++)
            {
                var strEmote = string.Empty;
                foreach (string emote in emoticons.Keys)
                {
                    if (inputText.Length - i >= emote.Length && emote.Equals(inputText.Substring(i, emote.Length), StringComparison.InvariantCultureIgnoreCase))
                    {
                        strEmote = emote;
                        break;
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
