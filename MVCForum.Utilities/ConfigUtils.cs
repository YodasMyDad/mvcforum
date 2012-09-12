using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace MVCForum.Utilities
{
    public static class ConfigUtils
    {
        /// <summary>
        /// Gets application setting
        /// </summary>
        /// <param name="appSettingame"></param>
        /// <returns>App setting value</returns>
        public static string GetAppSetting(string appSettingame)
        {
            if (appSettingame.IsNullEmpty())
            {
                throw new ApplicationException("AppSetting is null");
            }
            return ConfigurationManager.AppSettings[appSettingame];
        }

        /// <summary>
        /// Gets application setting or returns a default value on nay exception
        /// </summary>
        /// <param name="appSettingame"></param>
        /// <param name="defaultValue"></param>
        /// <returns>App setting value</returns>
        public static string GetAppSetting(string appSettingame, string defaultValue)
        {
            var appValue = defaultValue;

            try
            {
                if (!appSettingame.IsNullEmpty())
                {
                    appValue = ConfigurationManager.AppSettings[appSettingame];

                    if (appValue == null) // Not found in app settings
                    {
                        appValue = defaultValue;
                    }
                }
            }
            catch
            {
                // Empty - default value
            }

            return appValue;
        }

        /// <summary>
        /// Gets application setting and returns as Int32
        /// </summary>
        /// <param name="appSettingame"></param>
        /// <param name="defaultValue"></param>
        /// <returns>App Setting Value</returns>
        public static int GetAppSettingInt32(string appSettingame, int defaultValue)
        {
            return GetAppSetting(appSettingame, defaultValue.ToString()).ToInt32();
        }

        /// <summary>
        /// Updates an app setting in the config based on name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static bool UpdateAppSetting(string name, string value)
        {
            try
            {
                var webConfigPath = HttpContext.Current.Server.MapPath("~/web.config");
                var xpathToSetting = string.Format("//add[@key='{0}']", name);
                var xDoc = new XmlDocument();
                xDoc.Load(HttpContext.Current.Server.MapPath("~/web.config"));
                var settingNodes = xDoc.GetElementsByTagName("appSettings");
                var appSettingNode = settingNodes[0].SelectSingleNode(xpathToSetting);
                if (appSettingNode != null && appSettingNode.Attributes != null)
                {
                    var idAttribute = appSettingNode.Attributes["value"];
                    if(idAttribute != null)
                    {
                        idAttribute.Value = value;
                        xDoc.Save(webConfigPath);
                        return true;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
