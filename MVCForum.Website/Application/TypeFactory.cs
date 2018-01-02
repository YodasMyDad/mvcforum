namespace MvcForum.Web.Application
{
    using System;
    using System.Web;

    public static class TypeFactory
    {
        public static T GetInstanceOf<T>(string type)
        {
            if (HttpContext.Current != null)
            {
                var key = string.Concat("type-", type);
                if (!HttpContext.Current.Items.Contains(key))
                {
                    var resolvedType = (T) Activator.CreateInstance(Type.GetType(type));
                    HttpContext.Current.Items.Add(key, resolvedType);
                }
                return (T) HttpContext.Current.Items[key];
            }
            return (T) Activator.CreateInstance(Type.GetType(type));
        }
    }
}