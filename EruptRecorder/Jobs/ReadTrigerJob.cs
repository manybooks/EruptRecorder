using System;
using System.Collections.Generic;
using System.Linq;
using EruptRecorder.Models;
using EruptRecorder.Settings;

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
            List<EventTrigger> trigers = ReadTriggerFile(timeOfLastRun);
            return trigers;
        }

        public List<EventTrigger> ReadTriggerFile(DateTime timeOfLastRun)
        {
            List<EventTrigger> result = new List<EventTrigger>();

            // csvファイルを開く
            using (var sr = new System.IO.StreamReader($"{inputFilePath}"))
            {
                try
                {
                    // ヘッダ行を飛ばす
                    sr.ReadLine();

                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        EventTrigger eventTriger = EventTrigger.Parse(line);
                        
                        // 前回読み込んだところまで読んだら強制的に終了する
                        if (eventTriger.timeStamp < timeOfLastRun)
                        {
                            break;
                        }

                        result.Add(eventTriger);
                    }
                }
                catch (System.Exception e)
                {
                    // ファイルを開くのに失敗したとき
                    System.Console.WriteLine(e.Message);
                }
                finally
                {
                    sr.Close();
                }
            }
            return result;
        }
    }
}
