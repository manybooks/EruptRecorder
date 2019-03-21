using System;
using System.Collections.Generic;
using System.Linq;
using EruptRecorder.Models;

namespace EruptRecorder.Jobs
{
    public class ReadTrigerJob
    {
        public string inputFilePath { get; set; }

        public ReadTrigerJob(string inputFilePath)
        {
            this.inputFilePath = inputFilePath;
        }

        public List<EventTrigger> Run(DateTime timeOfLastRun)
        {
            List<EventTrigger> trigers = ReadFile();
            return trigers.Where(triger => triger.timeStamp > timeOfLastRun).ToList();
        }

        public List<EventTrigger> ReadFile()
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
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                System.Console.WriteLine(e.Message);
            }
            return result;
        }
    }
}
