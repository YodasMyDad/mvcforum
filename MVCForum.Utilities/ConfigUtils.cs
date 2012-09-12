using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;

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
        /// Updates a specific app setting in the config
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// I noticed that nothing seemed to be written to the configuration file while debugging within Visual Studio, 
        /// but once I published the application it worked as expected.
        /// it’s because when you make that call vs is editing the vshost.exe.config file in the debug directory not the app.config in your project.
        public static bool UpdateAppSetting(string name, string value)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                config.AppSettings.Settings.Remove(name);
                config.AppSettings.Settings.Add(name, value);
                config.Save();

                //var oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //oConfig.AppSettings.Settings[name].Value = value;
                //oConfig.Save(ConfigurationSaveMode.Modified);
                //ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
