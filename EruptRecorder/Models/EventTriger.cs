using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EruptRecorder.Models
{
    public class EventTriger
    {
        public DateTime timeStamp { get; set; }
        public int flag { get; set; }

        public EventTriger(DateTime timeStamp, int flag)
        {
            this.timeStamp = timeStamp;
            this.flag = flag;
        }

        public static EventTriger Parse(string line)
        {
            var values = line.Split(',');

            DateTime timeStamp;
            int flag;

            if (DateTime.TryParse(values[0], out timeStamp) && int.TryParse(values[1], out flag))
            {
                return new EventTriger(timeStamp, flag);
            }

            throw new FormatException("入力ファイルの形式が不正です。入力ファイルは{yyyyMMddhhmmss},{index}が記載されたCSVファイルである必要があります。");
        }
    }
}
