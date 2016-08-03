﻿using System;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Website.Application
{
    public static class DatesUI
    {

        private static string GetLocalisedText(string key)
        {
            var locService = ServiceFactory.Get<ILocalizationService>();
            return locService.GetResourceString(key);
        }

        /// <summary>
        /// Returns a pretty date like Facebook
        /// </summary>
        /// <param name="date"></param>
        /// <returns>28 Days Ago</returns>
        public static string GetPrettyDate(string date)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time))
            {
                var span = DateTime.UtcNow.Subtract(time);
                var totalDays = (int)span.TotalDays;
                var totalSeconds = (int)span.TotalSeconds;
                if ((totalDays < 0) || (totalDays >= 0x1f))
                {
                    return DateUtils.FormatDateTime(date, "dd MMMM yyyy");
                }
                if (totalDays == 0)
                {
                    if (totalSeconds < 60)
                    {
                        return GetLocalisedText("Date.JustNow");
                    }
                    if (totalSeconds < 120)
                    {
                        return GetLocalisedText("Date.OneMinuteAgo");
                    }
                    if (totalSeconds < 0xe10)
                    {
                        return string.Format(GetLocalisedText("Date.MinutesAgo"), Math.Floor((double)(((double)totalSeconds) / 60.0)));
                    }
                    if (totalSeconds < 0x1c20)
                    {
                        return GetLocalisedText("Date.OneHourAgo");
                    }
                    if (totalSeconds < 0x15180)
                    {
                        return string.Format(GetLocalisedText("Date.HoursAgo"), Math.Floor((double)(((double)totalSeconds) / 3600.0)));
                    }
                }
                if (totalDays == 1)
                {
                    return GetLocalisedText("Date.Yesterday");
                }
                if (totalDays < 7)
                {
                    return string.Format(GetLocalisedText("Date.DaysAgo"), totalDays);
                }
                if (totalDays < 0x1f)
                {
                    return string.Format(GetLocalisedText("Date.WeeksAgo"), Math.Ceiling((double)(((double)totalDays) / 7.0)));
                }
            }
            return date;
        }
        /// <summary>
        /// Returns a pretty date for tooltip
        /// </summary>
        /// <param name="date"></param>
        /// <returns>String as date format "d MMMM yyyy"</returns>
        public static string GetFormattedDate(string date)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time))
            {
                var span = DateTime.UtcNow.Subtract(time);
                var totalDays = (int)span.TotalDays;
                var totalSeconds = (int)span.TotalSeconds;
                return DateUtils.FormatDateTime(date, "d MMMM yyyy");
            }
            return date;
        }
    }
}