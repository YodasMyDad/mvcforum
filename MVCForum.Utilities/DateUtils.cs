using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MVCForum.Utilities
{
    public static class DateUtils
    {
        /// <summary>
        /// Creates a date format of dd MMMM yyyu
        /// </summary>
        /// <param name="theDate"></param>
        /// <param name="removeYear"> </param>
        /// <returns>A formatted string</returns>
        public static string FormatLongDate(DateTime theDate, bool removeYear = false)
        {
            return removeYear ? theDate.ToString("dd MMMM") : theDate.ToString("dd MMMM yyyy");
        }

        /// <summary>
        /// Converts an object into a date
        /// </summary>
        /// <param name="theDate"></param>
        /// <returns>The date, or a date representing now if object cannot be parsed</returns>
        public static DateTime ParseDate(object theDate)
        {
            DateTime date;
            return DateTime.TryParse(theDate.ToString(), out date) ? date : DateTime.Now;
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
                var span = DateTime.Now.Subtract(time);
                var totalDays = (int) span.TotalDays;
                var totalSeconds = (int) span.TotalSeconds;
                if ((totalDays < 0) || (totalDays >= 0x1f))
                {
                    return FormatDateTime(date, "dd MMMM yyyy");
                }
                if (totalDays == 0)
                {
                    if (totalSeconds < 60)
                    {
                        return "just now";
                    }
                    if (totalSeconds < 120)
                    {
                        return "1 minute ago";
                    }
                    if (totalSeconds < 0xe10)
                    {
                        return string.Format("{0} minutes ago", Math.Floor((double) (((double) totalSeconds)/60.0)));
                    }
                    if (totalSeconds < 0x1c20)
                    {
                        return "1 hour ago";
                    }
                    if (totalSeconds < 0x15180)
                    {
                        return string.Format("{0} hours ago", Math.Floor((double) (((double) totalSeconds)/3600.0)));
                    }
                }
                if (totalDays == 1)
                {
                    return "yesterday";
                }
                if (totalDays < 7)
                {
                    return string.Format("{0} days ago", totalDays);
                }
                if (totalDays < 0x1f)
                {
                    return string.Format("{0} weeks ago", Math.Ceiling((double) (((double) totalDays)/7.0)));
                }
            }
            return date;
        }

        public static string FormatDateTime(string date, string format)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time) && !string.IsNullOrEmpty(format))
            {
                format = Regex.Replace(format, @"(?<!\\)((\\\\)*)(S)", "$1" + GetDayNumberSuffix(time));
                return time.ToString(format);
            }
            return string.Empty;
        }

        private static string GetDayNumberSuffix(DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 0x15:
                case 0x1f:
                    return @"\s\t";

                case 2:
                case 0x16:
                    return @"\n\d";

                case 3:
                case 0x17:
                    return @"\r\d";
            }
            return @"\t\h";
        }

        public static string GetCurrentMonthName()
        {
            return string.Format("{0:MMMM}", DateTime.Now);
        }

        /// <summary>
        /// Returns the date of the monday of a specific week in a specific year
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        private static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int) jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1,
                                                                              CultureInfo.CurrentCulture.DateTimeFormat.
                                                                                  CalendarWeekRule,
                                                                              CultureInfo.CurrentCulture.DateTimeFormat.
                                                                                  FirstDayOfWeek);
            if (firstWeek <= 1)
            {
                weekOfYear -= 1;
            }
            return firstMonday.AddDays(weekOfYear*7);
        }

        /// <summary>
        /// Returns the time difference in minutes between two date times
        /// </summary>
        /// <param name="dateone"></param>
        /// <param name="datetwo"></param>
        /// <returns></returns>
        public static double TimeDifferenceInMinutes(DateTime dateone, DateTime datetwo)
        {
            var duration = dateone - datetwo;
            return duration.TotalMinutes;
        }


        /// <summary>
        /// Gets a specific day of the weeks date and the next consectuive Nth days dates, example would be every Fridays date for the current month
        /// </summary>
        /// <param name="dt">The date to start from (usually DateTime.Now)</param>
        /// <param name="weekday">The day of the week to look for</param>
        /// <param name="amounttoshow">How man to return, defaults to next 4</param>
        /// <returns>Returns the date of each of the days</returns>
        public static IEnumerable<DateTime> ReturnNextNthWeekdaysOfMonth(DateTime dt, DayOfWeek weekday, int amounttoshow = 4)
        {
            var days =
                Enumerable.Range(1, DateTime.DaysInMonth(dt.Year, dt.Month)).Select(
                    day => new DateTime(dt.Year, dt.Month, day));

            var weekdays = from day in days
                           where day.DayOfWeek == weekday
                           orderby day.Day ascending
                           select day;

            return weekdays.Take(amounttoshow);
        }

        /// <summary>
        /// Gets a specific day of the weeks date and the next consectuive Nth days dates, example would be every Fridays date for however many you want to show
        /// </summary>
        /// <param name="dt">The date to start from (usually DateTime.Now)</param>
        /// <param name="weekday">The day of the week to look for</param>
        /// <param name="amounttoshow">How man to return, defaults to next 4</param>
        /// <returns>Returns the date of each of the days</returns>
        public static IEnumerable<DateTime> ReturnNextNthWeekdays(DateTime dt, DayOfWeek weekday, int amounttoshow = 4)
        {
            // Find the first future occurance of the day.
            while (dt.DayOfWeek != weekday)
                dt = dt.AddDays(1);

            // Create the entire range of dates required. 
            return Enumerable.Range(0, amounttoshow).Select(i => dt.AddDays(i * 7));
        }
    }
}
