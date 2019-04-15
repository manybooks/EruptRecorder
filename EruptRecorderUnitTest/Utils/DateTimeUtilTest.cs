using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EruptRecorder.Utils;

namespace EruptRecorderUnitTest.Utils
{
    [TestClass]
    public class DateTimeUtilTest
    {
        static int year = 2019;
        static int month = 4;
        static int day = 1;

        [DataRow(1, 2, 3, 55, 1, 2, 3, 0)]
        [DataRow(1, 2, 3, 1, 1, 2, 3, 0)]
        [DataRow(1, 2, 3, 999, 1, 2, 3, 0)]
        [TestMethod]
        public void RoundDownMillisecondsTest(int hour, int minute, int second, int millisecond, int ex_hour, int ex_minute, int ex_second, int ex_millisecond)
        {
            DateTime origin = new DateTime(year, month, day, hour, minute, second, millisecond);
            DateTime roundDownd = DateTimeUtil.RoundDownMilliseconds(origin);
            DateTime expected = new DateTime(year, month, day, ex_hour, ex_minute, ex_second, ex_millisecond);

            Assert.AreEqual(expected, roundDownd);
        }

        [TestMethod]
        public void RoundDownMillisecondsWithCurrentTimeTest()
        {
            DateTime origin = DateTime.Now;
            DateTime roundDownd = DateTimeUtil.RoundDownMilliseconds(origin);
            DateTime expected = 
                new DateTime(origin.Year, origin.Month, origin.Day, origin.Hour, origin.Minute, origin.Second, 0);

            Assert.AreEqual(expected, roundDownd);
        }

        [DataRow(1, 2, 3, 55, 1, 2, 4, 0)]
        [DataRow(1, 2, 3, 1, 1, 2, 4, 0)]
        [DataRow(1, 2, 3, 999, 1, 2, 4, 0)]
        [TestMethod]
        public void RoundUpMillisecondsTest(int hour, int minute, int second, int millisecond, int ex_hour, int ex_minute, int ex_second, int ex_millisecond)
        {
            DateTime origin = new DateTime(year, month, day, hour, minute, second, millisecond);
            DateTime roundUpped = DateTimeUtil.RoundUpMilliseconds(origin);
            DateTime expected = new DateTime(year, month, day, ex_hour, ex_minute, ex_second, ex_millisecond);

            Assert.AreEqual(expected, roundUpped);
        }

        [TestMethod]
        public void RoundUpMillisecondsWithCurrentTimeTest()
        {
            DateTime origin = DateTime.Now;
            DateTime roundUpped = DateTimeUtil.RoundUpMilliseconds(origin);
            DateTime expected =
                new DateTime(origin.Year, origin.Month, origin.Day, origin.Hour, origin.Minute, origin.Second + 1, 0);

            Assert.AreEqual(expected, roundUpped);
        }
    }
}
