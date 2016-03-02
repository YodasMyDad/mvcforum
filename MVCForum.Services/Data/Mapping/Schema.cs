using System.Configuration;

namespace MVCForum.Services.Data.Mapping
{
    internal static class Schema
    {
        public static string Name
        {
            get
            {
                var owner = ConfigurationManager.AppSettings["MVCForum:SchemaName"];
                return string.IsNullOrEmpty(owner) ? "dbo" : owner;
            }
        }
    }
}
