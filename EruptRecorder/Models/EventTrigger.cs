﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EruptRecorder.Models
{
    public class EventTrigger
    {
        public DateTime timeStamp { get; set; }
        public int flag { get; set; }

        public EventTrigger(DateTime timeStamp, int flag)
        {
            this.timeStamp = timeStamp;
            this.flag = flag;
        }

        public static EventTrigger Parse(string line)
        {
            var values = line.Split(',');

            DateTime timeStamp;
            int flag;

            if (DateTime.TryParseExact(values[0].Substring(0, 16), "yyyyMMddHHmmssff", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out timeStamp) &&
                int.TryParse(values[1], out flag))
            {
                return new EventTrigger(timeStamp, flag);
            }

            throw new Exception($"予期せぬ入力トリガーフォーマット'{line}'");
        }

        public static bool IsValidInputLine(string line)
        {
            if (!line.Contains(","))
            {
                return false;
            }
            var values = line.Split(',');
            if (values[0].Length < 16)
            {
                return false;
            }
            DateTime timeStamp;
            bool isValidDatetime = DateTime.TryParseExact(values[0].Substring(0, 16), "yyyyMMddHHmmssff", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out timeStamp);
            if (!isValidDatetime)
            {
                return false;
            }
            int flag;
            bool isValidFlag = int.TryParse(values[1], out flag);
            if (!isValidFlag)
            {
                return false;
            }
            if (flag != 0 && flag != 1)
            {
                return false;
            }
            return true;
        }
    }
}
