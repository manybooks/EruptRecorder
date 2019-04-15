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
            int millisecondsToReduce = origin.Millisecond;
            return origin.AddMilliseconds(-1 * millisecondsToReduce);
        }

        public static DateTime RoundUpMilliseconds(DateTime origin)
        {
            int millisecondsToReduce = origin.Millisecond;
            DateTime roundDowned = origin.AddMilliseconds(-1 * millisecondsToReduce);
            return roundDowned.AddSeconds(1);
        }
    }
}
