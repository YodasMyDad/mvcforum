using System;
using System.Web;

namespace MVCForum.Website.Application
{
    public static class MobileExtensions
    {
        public static bool UserAgentContains(this HttpContextBase c, string agentToFind)
        {
            return (c.Request.UserAgent != null && c.Request.UserAgent.IndexOf(agentToFind, StringComparison.OrdinalIgnoreCase) > 0);
        }

        public static bool IsMobileDevice(this HttpContextBase c)
        {
          if (c.Request.Browser.IsMobileDevice || c.UserAgentContains("Android")
                || c.UserAgentContains("iPhone") || c.UserAgentContains("iPod")
                || c.UserAgentContains("Windows Phone") || c.UserAgentContains("Blackberry")
                || c.UserAgentContains("iemobile") || c.UserAgentContains("iPad"))
            {
                // We know its a mobile device so now work out if we think its a tablet or not
                if (c.UserAgentContains("ipad") || (c.UserAgentContains("android") && !c.UserAgentContains("mobile")))
                {
                    // Its a tablet so return false
                    return false;
                }

                return true;
            }
            return false;

        }        
    }
}