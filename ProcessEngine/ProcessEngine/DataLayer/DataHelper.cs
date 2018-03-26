using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DataLayer
{
    public class DataHelper
    {
        private static string isoPattern = "yyyy-MM-dd'T'HH:mm:ss.FFFK";
        private static string yyyyMMdd_HHmmsPattern = "yyyy-MM-dd HH:mm:ss";
        private static string ddMMyyyy_HHmmsPattern = "dd.MM.yyyy HH:mm:ss";
        private static string MM_dd_yyyy_HHmmsPattern = "M/d/yyyy h:mm:ss tt";
        public static string DateTimeToIso8601(DateTime datetime)
        {

            DateTimeOffset dateTimeOffset = datetime;
            return FormatIso8601(dateTimeOffset);
        }

        public static DateTime Iso8601ToDateTime(string iso8601)
        {
            // Convert UTC to DateTime value
            DateTimeOffset dateTimeOffset = ParseIso8601(iso8601);

            // get DateTime from your machine
            DateTimeOffset myoffset = new DateTimeOffset(DateTime.Now);

            TimeSpan difference = myoffset.Offset - dateTimeOffset.Offset;

            return dateTimeOffset.DateTime.Add(difference);
        }

        private static string FormatIso8601(DateTimeOffset dto)
        {
            return dto.ToString(isoPattern, CultureInfo.InvariantCulture);
        }

        private static DateTimeOffset ParseIso8601(string iso8601String)
        {
            return DateTimeOffset.ParseExact(
                iso8601String,
                new string[] { isoPattern, yyyyMMdd_HHmmsPattern, ddMMyyyy_HHmmsPattern, MM_dd_yyyy_HHmmsPattern },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None);
        }

        
        public static string BuildLogContextPrefix(string uniqueID, string wode, string woin, string activityInstanceID, long elapsedMilliseconds = -1)
        {
            string prefixTemplate = "[RunTimeGuid:{0}; WODE:{1}; WOIN:{2}; ActivityInstance:'{3}';Elapsed:{4}]:";
            string result = "";
            try
            {
                if (uniqueID == null) uniqueID = "null";
                if (woin == null) woin = "null";
                if (wode == null) wode = "null";
                if (activityInstanceID == null) activityInstanceID = "null";

                result = string.Format(prefixTemplate,
                    uniqueID,
                    wode,
                    woin,
                    activityInstanceID,
                    elapsedMilliseconds
                    );
            }
            catch
            {
                result = "[Error building prefix]:";
            }
            return result;
        }
        public static string BuildLogContextPrefix(string uniqueID, string wode, string woin)
        {
            return BuildLogContextPrefix(
                uniqueID: uniqueID,
                wode: wode,
                woin: woin,
                activityInstanceID: "n/a");

        }
        public static string BuildLogContextPrefix(string uniqueID)
        {
            return BuildLogContextPrefix(
                uniqueID: uniqueID,
                wode: "n/a",
                woin: "n/a",
                activityInstanceID: "n/a");
        }
    }
}
