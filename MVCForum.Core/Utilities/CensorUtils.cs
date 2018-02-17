namespace MvcForum.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class CensorUtils
    {
        public IList<string> CensoredWords { get; }

        public CensorUtils(IEnumerable<string> censoredWords)
        {
            if (censoredWords == null)
                throw new ArgumentNullException(nameof(censoredWords));

            CensoredWords = new List<string>(censoredWords);
        }

        public string CensorText(string text)
        {
            if (text == null)
            {
                return null;
            }

            var censoredText = text;

            foreach (var censoredWord in CensoredWords)
            {
                var regularExpression = ToRegexPattern(censoredWord);
                censoredText = Regex.Replace(censoredText, regularExpression, StarCensoredMatch, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            return censoredText;
        }

        private static string StarCensoredMatch(Match m)
        {
            var word = m.Captures[0].Value;
            var bannedCharactor = ForumConfiguration.Instance.BannedWordReplaceCharactor;
            if (!string.IsNullOrWhiteSpace(bannedCharactor))
            {
                return new string(Convert.ToChar(bannedCharactor), word.Length);
            }
            return new string(Convert.ToChar("*"), word.Length);
        }

        private static string ToRegexPattern(string wildcardSearch)
        {
            var regexPattern = Regex.Escape(wildcardSearch);

            regexPattern = regexPattern.Replace(@"\*", ".*?");
            regexPattern = regexPattern.Replace(@"\?", ".");

            if (regexPattern.StartsWith(".*?"))
            {
                regexPattern = regexPattern.Substring(3);
                regexPattern = $@"(^\b)*?{regexPattern}";
            }

            regexPattern = $@"\b{regexPattern}\b";

            return regexPattern;
        }
    }
}
