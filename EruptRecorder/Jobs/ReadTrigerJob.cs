using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using EruptRecorder.Models;
using EruptRecorder.Settings;
using log4net;

namespace EruptRecorder.Jobs
{
    public class ReadTrigerJob
    {
        public string inputFilePath { get; set; }
        ILog logger { get; set; }

        public ReadTrigerJob(string inputFilePath, ILog logger)
        {
            this.inputFilePath = inputFilePath;
            this.logger = logger;
        }

        public List<EventTrigger> Run(DateTime timeOfLastRun)
        {
            List<EventTrigger> trigers = ReadTriggerFile(timeOfLastRun);
            return trigers.Where(trigger => trigger.timeStamp >= timeOfLastRun).ToList();
        }

        public List<EventTrigger> ReadTriggerFile(DateTime timeOfLastRun)
        {
            List<EventTrigger> result = new List<EventTrigger>();

            try
            {
                // csvファイルを開く
                using (var sr = new System.IO.StreamReader($"{inputFilePath}"))
                {
                    // ヘッダ行を飛ばす
                    sr.ReadLine();

                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        EventTrigger eventTriger = EventTrigger.Parse(line);

                        result.Add(eventTriger);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // 入力ファイルが存在しなかったとき
                logger.Error($"トリガーファイルとして指定された{inputFilePath}が存在しません。");
            }
            catch (FormatException fe)
            {
                // 入力ファイルの形式が不正だったとき
                logger.Error($"{fe.Message}");
            }
            catch (System.Exception e)
            {
                // その他想定していない例外
                logger.Error(e.Message);
            }
            return result;
        }
    }
}
