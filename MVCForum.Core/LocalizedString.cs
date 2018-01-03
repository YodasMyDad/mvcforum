namespace MvcForum.Core
{
    using System;
    using System.Web;

    public class LocalizedString : MarshalByRefObject, IHtmlString
    {
        public LocalizedString(string localized)
        {
            Text = localized;
        }

        public LocalizedString(string localized, string scope, string textHint, object[] args)
        {
            Text = localized;
            Scope = scope;
            TextHint = textHint;
            Args = args;
        }

        public string Scope { get; }

        public string TextHint { get; }

        public object[] Args { get; }

        public string Text { get; }

        public string ToHtmlString()
        {
            return Text;
        }

        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue)
        {
            return string.IsNullOrWhiteSpace(text) ? defaultValue : new LocalizedString(text);
        }

        public override string ToString()
        {
            return Text;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (Text != null)
            {
                hashCode ^= Text.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var that = (LocalizedString) obj;

            return string.Equals(Text, that.Text);
        }
    }
}