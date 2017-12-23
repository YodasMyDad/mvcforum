using System;

namespace MVCForum.Utilities
{
    public class GuidComb
    {
        /// <summary>
        /// Generates a sequential guid
        /// Based on http://stackoverflow.com/questions/665417/sequential-guid-in-linq-to-sql
        /// </summary>
        /// <returns></returns>
        public static Guid GenerateComb()
        {
            // Fill the destination array with a guid - only the last 6 bytes of a guid are
            // evaluated for sorting on SQL Server, and this algorithm will later overwrite those with
            // 6 bytes that are related to time, and therefore the guids are in generation order as far
            // as SQL Server is concerned.
            // Putting an actual guid in the destination array first helps ensure uniqueness across
            // the remaining bytes. See http://msdn.microsoft.com/en-us/library/ms254976.aspx
            var destinationArray = Guid.NewGuid().ToByteArray();

            // Get clock ticks since 1900 and convert to byte array (we will use last 4 bytes later)
            var time = new DateTime(1900, 1, 1);
            var now = DateTime.UtcNow;
            var ticksSince1900 = new TimeSpan(now.Ticks - time.Ticks);
            var bytesFromClockTicks = BitConverter.GetBytes(ticksSince1900.Days);

            // Get milliseconds from time of day and convert to byte array (we will use last 2 bytes later)
            var timeOfDay = now.TimeOfDay;
            var bytesFromMilliseconds = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333)); // Note that SQL Server is accurate to 3.33 millisecond so we divide by 3.333333,
            // makes us compatible with NEWSEQUENTIALID. Not sure that this is useful...

            // Reverse bytes for storage in SQL server
            Array.Reverse(bytesFromClockTicks);
            Array.Reverse(bytesFromMilliseconds);

            // Replace the last 6 bytes of our Guid. These are the ones SQL server will use when comparing guids
            Array.Copy(bytesFromClockTicks, bytesFromClockTicks.Length - 2, destinationArray, destinationArray.Length - 6, 2);
            Array.Copy(bytesFromMilliseconds, bytesFromMilliseconds.Length - 4, destinationArray, destinationArray.Length - 4, 4);

            return new Guid(destinationArray);
        }

    }
}
