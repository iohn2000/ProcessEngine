using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Utils;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class DateTimeParseTests
    {
        [TestMethod]
        public void TestDateTimeToIso8601()
        {
            DateTime dateTime = DateTime.Now;

            string is8601 = DateTimeHelper.DateTimeToIso8601(dateTime);

            // shift the time to another timezone
            int startIndex = is8601.Length - 5;
            string hours = is8601.Substring(startIndex, 2);
            int inthours = Convert.ToInt32(hours);
            inthours = inthours + 2;
            string newIso = is8601.Substring(0, startIndex) + inthours.ToString("D2") + is8601.Substring(startIndex + 2);

            DateTime dateTimeConverted = DateTimeHelper.Iso8601ToDateTime(newIso);
            dateTimeConverted = dateTimeConverted.AddHours(2);
            DateTime compare1 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
            DateTime compare2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);

            Assert.IsTrue(DateTime.Equals(compare1, compare2));
        }
    }
}
