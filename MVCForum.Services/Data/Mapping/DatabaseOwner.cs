using System.Configuration;

namespace MVCForum.Services.Data.Mapping
{
    internal static class DatabaseOwner
    {
        public static string Name
        {
            get
            {
                var owner = ConfigurationManager.AppSettings["MVCForum:DatabaseOwner"];
                return string.IsNullOrEmpty(owner) ? "dbo" : owner;
            }
        }
    }
}
