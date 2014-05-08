Getting started with Unity.WebAPI
---------------------------------

To get started, just add a call to UnityConfig.RegisterComponents() in the Application_Start method of Global.asax.cs 
and the Web API framework will then use the Unity.WebAPI DependencyResolver to resolve your components.

e.g.
 
public class WebApiApplication : System.Web.HttpApplication
{
  protected void Application_Start()
  {
    AreaRegistration.RegisterAllAreas();
    UnityConfig.RegisterComponents();                           // <----- Add this line
    GlobalConfiguration.Configure(WebApiConfig.Register);
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
  }           
}  

Add your Unity registrations in the RegisterComponents method of the UnityConfig class. All components that implement IDisposable should be 
registered with the HierarchicalLifetimeManager to ensure that they are properly disposed at the end of the request.