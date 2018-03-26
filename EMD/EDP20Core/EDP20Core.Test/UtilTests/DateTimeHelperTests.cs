using Kapsch.IS.EDP.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Bogus;
using System.Text.RegularExpressions;
using System.Globalization;

namespace EDP20Core.Test.UtilTests
{
    [TestFixture, Category("UtilTests"), Category("DateTimeHelperTests")]
    public class DateTimeHelperTests
    {

        [Test, TestCaseSource(typeof(DateTimeTestCases), "DateTimeToIso8601TestCases")]
        public string DateTimeToIso8601Test(DateTime dt)
        {
            return DateTimeHelper.DateTimeToIso8601(dt);
        }

        [Ignore("Doesn't work because of daylight-saving-time results in an 'wrong' date-time")]
        [Test, TestCaseSource(typeof(DateTimeTestCases), "Iso8601ToDateTimeTestCases")]
        public DateTime Iso8601ToDateTimeTest(string isoDt)
        {
            return DateTimeHelper.Iso8601ToDateTime(isoDt);
        }

        [Test, TestCaseSource(typeof(TestStrings), "Validation")]
        public void Iso8601ToDateTimeTestValidationTest(string dt)
        {
            Assert.That(() =>
            {
                DateTime parsedDt = DateTimeHelper.Iso8601ToDateTime(dt);
            }, Throws.InstanceOf<ArgumentException>()
                  .Or.InstanceOf<ArgumentNullException>());
        }

    }

    public static class DateTimeTestCases
    {
        private static DateTime GetDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0) => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

        private static DateTime GetLocalDateTime(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
        }

        public static IEnumerable<TestCaseData> DateTimeToIso8601TestCases
        {
            get
            {
                yield return new TestCaseData(GetDateTime(2299, 12, 31)).Returns("2299-12-31T00:00:00+00:00");
                yield return new TestCaseData(GetDateTime(2299, 12, 31, 12)).Returns("2299-12-31T12:00:00+00:00");
                yield return new TestCaseData(GetDateTime(2017, 1, 20, 6, 33, 30)).Returns("2017-01-20T06:33:30+00:00");
                yield return new TestCaseData(GetDateTime(1999, 12, 31, 12)).Returns("1999-12-31T12:00:00+00:00");
                yield return new TestCaseData(GetDateTime(1950, 12, 31, 12)).Returns("1950-12-31T12:00:00+00:00");
            }
        }

        public static IEnumerable<TestCaseData> Iso8601ToDateTimeTestCases
        {
            get
            {
                yield return new TestCaseData("2299-12-31T00:00:00+00:00").Returns(GetDateTime(2299, 12, 31).GetLocalDateTime()); //Does not work with daylight saving time (don't know how to fix)
            }
        }

        public static IEnumerable<TestCaseData> IsDateTimeEqualTestCases
        {
            get
            {
                yield return new TestCaseData(GetDateTime(2299, 12, 31), GetDateTime(2299, 12, 31), 1000).Returns(true);
                yield return new TestCaseData(GetDateTime(2299, 12, 31), GetDateTime(2299, 12, 31, 0, 0, 1), 1000).Returns(false)
                    .SetDescription("Considered unequal if difference is equal to the given tolerance");
                yield return new TestCaseData(GetDateTime(2299, 12, 31), GetDateTime(2000, 5, 7), 500).Returns(false);
            }
        }
    }
}
