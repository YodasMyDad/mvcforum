
Thank you for installing MVC Forum!

Until I become a master of PowerShell, and finish a proper install script for the NuGet package,
there's a few things you need to do after installing the package.

First off, if Ninject has created an App_Start folder containing a class file (NinjectWebCommon.cs), please delete this file!

Next off you need to take a look at your web.config file. You should make sure that the system.web/membership section only
exists once in the file, and that it contains the following providers:

    <membership defaultProvider="ProviderWrapper">
      <providers>
        <add name="ProviderWrapper" type="mvcForum.Web.Providers.MembershipProviderWrapper" WrappedProvider="TheRealProvider" />
        <add name="TheRealProvider" type="MVCBootstrap.Web.Security.SimpleMembershipProvider" applicationName="mvcForum" minRequiredPasswordLength="6" />
      </providers>
    </membership>

You're free to use your own provider instead of our SimpleMembershipProvider, but it is important for MVC Forum to work,
that you use the MembershipProviderWrapper as the active provider. Just add you're own and put the name of that provider
in the WrappedProvider attribute.

Also take a look at the system.web/roleManager section, it needs to be enabled, exist only once in the config file and looking like this:

    <roleManager enabled="true" defaultProvider="SimpleProvider">
      <providers>
        <clear />
        <add name="SimpleProvider" type="MVCBootstrap.Web.Security.SimpleRoleProvider" />
      </providers>
    </roleManager>

We do not (yet) support profiles, so remove that section please, or use your own.

As the last thing to change in the web.config file, you'll need to put in a connection string that works for your set-up.
Locate the connectionStrings section and change the content of the connection string for the mvcForum.DataProvider.MainDB one.

Now it's time to take a look at the global.asax file.

First off you need to add a parameter to the MapRoute method call in the RegisterRoutes method. This is always needed when
you have several controllers with the same name. This often happens when you use areas (MVC Forum does).

So make sure to add a namespace to your call:

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				new String[] { "yourmvcappnamespace.Controllers" }
			);

Secondly you need to configure MVC Forum to run, so in the Application_Start method, at the very top,
you should add this piece of code:

		protected void Application_Start() {
			mvcForum.Web.ApplicationConfiguration.Initialize();

			// Whatever other code you need
		}

This configures MVC Forum to use Entity Framework for data access, Lucene.Net as search engine, and Ninject for
dependency injection. It also adds the standard add-ons we've created for anti-spam measures.

Read more about what this does and how you can exchange our services with your own, or just add your own registrations
into MVC Forum's dependency injection framework.

http://mvcbootstrap.codeplex.com/wikipage?title=How%20To

Create a topnavigation.cshtml partial view, put it in the /views/shared folder.



For ASP.NET MVC 4 you need to do a few more things to get it working.

Locate the web.config files in the Areas/Forum/Views and Areas/ForumAdmin/Views folders.

In both the files locate this line:

				<add namespace="mvcForum.Web.Extensions.Mvc3" />

Delete it.

Add the following snippet to the runtime/assemblyBinding section of the main web.config file:

			<dependentAssembly>
				<assemblyIdentity name="System.Web.WebPages.Razor" publicKeyToken="31bf3856ad364e35" culture="neutral" />
				<bindingRedirect oldVersion="0.0.0.0-2.0.0.0" newVersion="2.0.0.0" />
			</dependentAssembly>



That's it! You can visit your new forum at http://yoursite/forum

Enjoy and please give us feedback!!
