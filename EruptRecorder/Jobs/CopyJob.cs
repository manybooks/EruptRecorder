using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EruptRecorder.Models;
using EruptRecorder.Settings;
using EruptRecorder.Logging;
using log4net;

namespace EruptRecorder.Jobs
{
    public class CopyJob
    {
        private ILog logger { get; set; }

        public CopyJob(ILog logger)
        {
            this.logger = logger;
        }

        public void Run(List<EventTrigger> trigers, CopySetting copySetting, RecordingSetting recordingSetting, ILog logger)
        {
            if (!copySetting.isActive) return;
            if (trigers == null || trigers?.Count() == 0)
            {
                logger.Info($"最後にジョブを起動した時刻{recordingSetting.timeOfLastRun}から現在までで、インデックス{copySetting.index}のコピーを要求する入力データはありませんでした。");
                return;
            }
            List<EventTrigger> trigersToCheck = trigers.OrderBy(triger => triger.timeStamp)
                                                       .ToList();
            List<CopyCondition> copyConditions = GetCopyConditionsFrom(trigersToCheck, copySetting.index, recordingSetting.minutesToGoBack);
            List<FileInfo> filesToCopy = GetCopyTargetFiles(copyConditions, copySetting);
            if (filesToCopy?.Count() == 0)
            {
                string prefixCondition = (string.IsNullOrEmpty(copySetting.prefix)) ? string.Empty : $"名前が'{copySetting.prefix}'で始まり、";
                logger.Warn($"コピー元のディレクトリ{copySetting.srcDir}に、{prefixCondition}拡張子が'{copySetting.fileExtension}'のファイルが見つかりませんでした。");
                return;
            }

            bool doneSuccessfully = ExecuteCopy(filesToCopy, copySetting);
            if (doneSuccessfully)
            {
                logger.Info($"インデックス{copySetting.index}のコピーが終了しました。");
            }
            else
            {
                logger.Error($"インデックス{copySetting.index}のコピーに失敗しました。");
            }
        }


        public List<CopyCondition> MergeTrigers(List<EventTrigger> eventTrigers)
        {
            return new List<CopyCondition>();
        }

        public List<CopyCondition> GetCopyConditionsFrom(List<EventTrigger> eventTriggers, int targetIndex, int minutesToGoBack)
        {
            if (eventTriggers?.Count() < 1) throw new ArgumentException("対象となるトリガーがありませんでした。");

            List<CopyCondition> copyConditions = new List<CopyCondition>();
            CopyCondition currentCondition = new CopyCondition();
            DateTime currentTo = eventTriggers.FirstOrDefault().timeStamp;
            bool isActive = false;

            foreach (EventTrigger trigger in eventTriggers)
            {
                if (isActive)
                {
                    // Deactivate開始
                    if (trigger.flag != targetIndex)
                    {
                        currentCondition.to = currentTo;
                        copyConditions.Add(currentCondition);

                        // 初期化
                        currentCondition = new CopyCondition();
                        isActive = false;
                    }
                }
                else
                {
                    // Activate開始
                    if (trigger.flag == targetIndex)
                    {
                        isActive = true;
                        currentCondition.from = trigger.timeStamp.AddMinutes(-1 * minutesToGoBack);
                    }
                }

                if (trigger.flag == targetIndex)
                {
                    // Deactivate時に設定する遡り終了時刻を設定
                    currentTo = trigger.timeStamp.AddSeconds(1); // DateTime型だとミリ秒考慮ができず、最新のファイルがコピーされない可能性があるため、1秒加算する
                }
            }

            // Activeのまま抜けた場合のチェック (ex)現在進行形で噴火中など
            if (isActive)
            {
                currentCondition.to = currentTo;
                copyConditions.Add(currentCondition);
            }
            return copyConditions;
        }

        public List<FileInfo> GetCopyTargetFiles(List<CopyCondition> copyConditions, CopySetting copySetting)
        {
            List<FileInfo> targetFiles = new List<FileInfo>();
            try
            {
                DirectoryInfo srcDirectory = new DirectoryInfo(copySetting.srcDir);
                foreach(CopyCondition copyCondition in copyConditions)
                {
                    var filesToCopy = srcDirectory.GetFiles()
                                                  .Where(f => copyCondition.from <= f.CreationTime && f.CreationTime <= copyCondition.to)
                                                  .Where(f => f.Name.StartsWith(copySetting.prefix))
                                                  .Where(f => Regex.IsMatch(f.Name, $".+\\.{copySetting.fileExtension}"))
                                                  .ToList();
                    targetFiles.AddRange(filesToCopy);
                }
            }
            catch (DirectoryNotFoundException)
            {
                logger.Error($"インデックス{copySetting.index}のコピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。");
                System.Windows.MessageBox.Show($"インデックス{copySetting.index}のコピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。", "コピー元フォルダ名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw new InvalidSettingsException($"インデックス{copySetting.index}のコピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。");
            }
            return targetFiles.Distinct(new ImageFileComparer()).ToList();
        }
        
        public bool ExecuteCopy(List<FileInfo> filesToCopy, CopySetting copySetting)
        {
            bool doneSuccessfully = true;
            try
            {
                logger.Info($"インデックス{copySetting.index}のファイルコピーを開始します。");
                DirectoryInfo destDirectory = new DirectoryInfo(copySetting.destDir);
                foreach(FileInfo f in filesToCopy)
                {
                    f.CopyTo(Path.Combine(destDirectory.FullName, f.Name), overwrite: true);
                    logger.Info($"ファイル '{f.Name}'を'{copySetting.srcDir}'から'{copySetting.destDir}'へコピーしました。");
                }
            }
            catch (DirectoryNotFoundException)
            {
                logger.Error($"インデックス{copySetting.index}のコピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。");
                System.Windows.MessageBox.Show($"インデックス{copySetting.index}のコピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。", "コピー先フォルダ名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw new InvalidSettingsException($"インデックス{copySetting.index}のコピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。");
            }
            catch (Exception ex)
            {
                logger.Error($"インデックス{copySetting.index}のファイルコピーに失敗しました。エラー内容は以下を参照してください。");
                logger.Error("**************************************************************************************************");
                logger.Error(ex.ToString());
                logger.Error("**************************************************************************************************");
                throw ex;
            }
            return doneSuccessfully;
        }
    }

    public class ImageFileComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo f1, FileInfo f2)
        {
            return f1.FullName == f2.FullName &&
                   f1.CreationTime == f2.CreationTime;
        }

        public int GetHashCode(FileInfo f)
        {
            return f.GetHashCode();
        }
    }

    public class CopyCondition
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        
        public bool IsValid()
        {
            if (from != null && to != null) return true;
            return false;
        }
    }
}