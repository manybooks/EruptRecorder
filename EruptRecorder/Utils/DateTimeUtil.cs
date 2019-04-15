using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EruptRecorder.Utils
{
    public static class DateTimeUtil
    {
        public static DateTime RoundDownMilliseconds(DateTime origin)
        {
            return new DateTime(origin.Year, origin.Month, origin.Day, origin.Hour, origin.Minute, origin.Second, 0);
        }

        public static DateTime RoundUpMilliseconds(DateTime origin)
        {
            DateTime roundDowned = new DateTime(origin.Year, origin.Month, origin.Day, origin.Hour, origin.Minute, origin.Second, 0);
            return roundDowned.AddSeconds(1);
        }
    }
}
