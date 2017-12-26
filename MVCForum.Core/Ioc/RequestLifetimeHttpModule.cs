namespace MvcForum.Core.Ioc
{
    using System.Web;

    internal class RequestLifetimeHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += (sender, e) => UnityDependencyResolver.DisposeOfChildContainer();
        }

        public void Dispose()
        {
            // nothing to do here
        }        
    }
}