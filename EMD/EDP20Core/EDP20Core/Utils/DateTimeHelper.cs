using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// Use the DateTimeHelper to get the correct DateTime between machines with Different Timezones 
    /// </summary>
    public static class DateTimeHelper
    {

        private static string isoPattern = "yyyy-MM-dd'T'HH:mm:ss.FFFK";

        /// <summary>
        /// Parses the given date/time to an string satisfying the ISO8601-standard.
        /// </summary>
        /// <param name="datetime">DateTime-object to be parsed</param>
        /// <returns>String representing the given DateTime in the format of ISO8601</returns>
        public static string DateTimeToIso8601(DateTime datetime)
        {

            DateTimeOffset dateTimeOffset = datetime;
            return FormatIso8601(dateTimeOffset);
        }

        /// <summary>
        /// Parses the given ISO8601-compliant string to DateTime
        /// </summary>
        /// <param name="iso8601"></param>
        /// <returns></returns>
        public static DateTime Iso8601ToDateTime(string iso8601)
        {
            if (iso8601 == null) throw new ArgumentNullException("iso8601");

            DateTimeOffset dateTimeOffset;

            // Convert UTC to DateTime value
            try
            {
                dateTimeOffset = ParseIso8601(iso8601);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(ex.Message, "iso8601");
            }

            // get DateTime from your machine
            DateTimeOffset myoffset = new DateTimeOffset(DateTime.Now);

            TimeSpan difference = myoffset.Offset - dateTimeOffset.Offset;

            return dateTimeOffset.DateTime.Add(difference);
        }

        /// <summary>
        /// Compares whether two DateTime-objects are equal. That is when the timespan between the two DateTimes is smaller than the given tolerance.
        /// </summary>
        /// <param name="first">First DateTime to compare.</param>
        /// <param name="second">Second DateTime to compare.</param>
        /// <param name="toleranceMilis">Tolerance in milliseconds. Default is 1000 ms.</param>
        /// <returns><see langword="true"/> if the two DateTimes are within the given tolerance otherwise <see langword="false"/>.</returns>
        public static bool IsDateTimeEqual(DateTime first, DateTime second, double toleranceMilis = 1000)
        {
            if (toleranceMilis < 0)
                toleranceMilis = 0;

            return Math.Abs((first - second).TotalMilliseconds) < toleranceMilis;
        }

        private static string FormatIso8601(DateTimeOffset dto)
        {
            return dto.ToString(isoPattern, CultureInfo.InvariantCulture);
        }

        private static DateTimeOffset ParseIso8601(string iso8601String)
        {
            return DateTimeOffset.ParseExact(
                iso8601String,
                new string[] { isoPattern },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None);
        }
    }

}
