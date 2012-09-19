using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.Website.Application
{
    public static class MobileExtensions
    {
        public static bool UserAgentContains(this HttpContextBase c, string agentToFind)
        {
            return (c.Request.UserAgent.IndexOf(agentToFind, StringComparison.OrdinalIgnoreCase) > 0);
        }

        public static bool IsMobileDevice(this HttpContextBase c)
        {
            return c.Request.Browser.IsMobileDevice
                   || c.UserAgentContains("Android")
                   || c.UserAgentContains("iPhone")
                   || c.UserAgentContains("Windows Phone");
        }        
    }
}