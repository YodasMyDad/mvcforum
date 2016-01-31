using System;

namespace MVCForum.Website.Application
{
    public static class TypeFactory
    {
        public static T GetInstanceOf<T>(string type)
        {
            return (T)Activator.CreateInstance(Type.GetType(type));
        }
    }
}
