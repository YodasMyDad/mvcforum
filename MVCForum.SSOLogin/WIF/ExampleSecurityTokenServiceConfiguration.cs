using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;

namespace MVCForum.SSOLogin.WIF
{
    public class ExampleSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {
        static readonly object syncRoot = new object();
        static string configKey = "MVCForumSecurityTokenServiceConfiguration";        

        /// <summary>
        /// A Singleton Implementation to retrieve an instance of ExampleSecurityTokenServiceConfiguration without recreating it.
        /// </summary>
        public static ExampleSecurityTokenServiceConfiguration Current
        {
            get
            {
                HttpApplicationState httpAppState = HttpContext.Current.Application;
                ExampleSecurityTokenServiceConfiguration thisConfig = httpAppState.Get(configKey) as ExampleSecurityTokenServiceConfiguration;
                if (thisConfig != null)
                {
                    return thisConfig;
                }
                lock (syncRoot)
                {
                    thisConfig = httpAppState.Get(configKey) as ExampleSecurityTokenServiceConfiguration;
                    if (thisConfig == null)
                    {
                        thisConfig = new ExampleSecurityTokenServiceConfiguration();
                        httpAppState.Add(configKey, thisConfig);
                    }
                }
                return thisConfig;
            }
        }


        public ExampleSecurityTokenServiceConfiguration() : base("MVCForumExampleSTS") //if you don't pass in a name here, login will fail, doesn't really matter what the name is, so long as it's globally unique.
        {                    
            this.SecurityTokenService = typeof(ExampleSecurityTokenService);
        }
    }
}