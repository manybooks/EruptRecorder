﻿using System;
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

        public List<EventTrigger> Run(DateTime? timeOfLastRun)
        {
            // 最終検出時刻がnullだった場合はDateTimeの初期値を使用
            if (timeOfLastRun == null)
            {
                timeOfLastRun = new DateTime();
            }

            List<EventTrigger> triggers = ReadTriggerFile();
            List<EventTrigger> addedTriggersFromLastRun = triggers.Where(trigger => trigger.timeStamp >= timeOfLastRun).ToList();
            return addedTriggersFromLastRun;
        }

        public List<EventTrigger> ReadTriggerFile()
        {
            List<EventTrigger> result = new List<EventTrigger>();

            try
            {
                // トリガーファイルを開く
                using (var sr = new System.IO.StreamReader($"{inputFilePath}"))
                {
                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line.Equals(string.Empty))
                        {
                            // 空行だった場合は飛ばす
                            continue;
                        }
                        if (!EventTrigger.IsValidInputLine(line))
                        {
                            // トリガーファイルの形式が不正だったとき
                            logger.Warn("トリガーファイルの形式が不正です。トリガーファイルの形式は{yyyyMMddhhmmssff} ,{index}である必要があります。");
                            logger.Warn($"対象データ:{line}");
                            continue;
                        }
                        EventTrigger eventTriger = EventTrigger.Parse(line);

                        result.Add(eventTriger);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // トリガーファイルが存在しなかったとき
                logger.Error($"トリガーとして指定されたファイル{inputFilePath}が存在しません。");
                System.Windows.MessageBox.Show($"トリガーとして指定されたファイル{inputFilePath}が存在しません。", "トリガーファイル名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw new InvalidSettingsException($"トリガーとして指定されたファイル{inputFilePath}が存在しません。");
            }
            catch (ArgumentException)
            {

                // トリガーファイルパスが空欄だったとき
                logger.Warn("トリガーファイル名が入力されていません。");
                throw new InvalidSettingsException("トリガーファイル名が入力されていません。");
            }
            catch (System.Exception ex)
            {
                // その他想定していない例外
                logger.Error(ex.Message);
                throw ex;
            }
            return result;
        }
    }
}
