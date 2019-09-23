using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EruptRecorder.Models;
using EruptRecorder.Settings;
using EruptRecorder.Logging;
using EruptRecorder.Utils;
using log4net;

namespace EruptRecorder.Jobs
{
    public class CopyJob
    {
        private ILog logger { get; set; }
        private const int IndexToCopy = 1;

        public CopyJob(ILog logger)
        {
            this.logger = logger;
        }

        public void Run(List<EventTrigger> trigers, CopySetting copySetting, RecordingSetting recordingSetting, ILog logger)
        {
            if (trigers == null || trigers?.Count() == 0)
            {
                logger.Info($"最後にジョブを起動した時刻{recordingSetting.timeOfLastRun}から現在までで、コピーを要求する入力データはありませんでした。");
                return;
            }
            List<EventTrigger> trigersToCheck = trigers.OrderBy(triger => triger.timeStamp)
                                                       .ToList();
            List<CopyCondition> copyConditions = GetCopyConditionsFrom(trigersToCheck, IndexToCopy, recordingSetting.minutesToGoBack);
            List<FileInfo> filesToCopy = GetCopyTargetFiles(copyConditions, copySetting);
            if (filesToCopy?.Count() == 0)
            {
                logger.Warn($"コピー元のディレクトリ{copySetting.srcDir}に、コピー対象のファイルが見つかりませんでした。");
                return;
            }

            bool doneSuccessfully = ExecuteCopy(filesToCopy, copySetting);
            if (doneSuccessfully)
            {
                logger.Info($"コピーが終了しました。");
            }
            else
            {
                logger.Error($"コピーに失敗しました。");
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
            DateTime currentTo = DateTimeUtil.RoundUpMilliseconds(eventTriggers.FirstOrDefault().timeStamp); // バッファをもってミリ秒を切り上げる
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
                        currentCondition.from = DateTimeUtil.RoundDownMilliseconds(trigger.timeStamp).AddMinutes(-1 * minutesToGoBack);
                    }
                }

                if (trigger.flag == targetIndex)
                {
                    // Deactivate時に設定する遡り終了時刻を設定
                    currentTo = DateTimeUtil.RoundUpMilliseconds(trigger.timeStamp); // バッファをもってミリ秒を切り上げる
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
            DirectoryInfo srcRootDir = new DirectoryInfo(copySetting.srcDir);
            if (!srcRootDir.Exists)
            {
                logger.Error($"設定画面で入力されたコピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。");
                System.Windows.MessageBox.Show($"コピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。", "コピー元フォルダ名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw new InvalidSettingsException($"コピー元フォルダ '{copySetting.srcDir}' が見つかりませんでした。");
            }
            foreach(CopyCondition copyCondition in copyConditions)
            {
                List<DirectoryInfo> directoriesToSeek = GetDirectoriesToSeek(copyCondition, srcRootDir);
                foreach(DirectoryInfo srcDirectory in directoriesToSeek)
                {
                    try
                    {
                        var filesToCopy = srcDirectory.GetFiles()
                                .Where(f => copyCondition.from <= f.CreationTime && f.CreationTime <= copyCondition.to)
                                .Where(f => copySetting.copyStartDateTime <= f.CreationTime && f.CreationTime <= copySetting.copyEndDateTime)
                                .ToList();
                        targetFiles.AddRange(filesToCopy);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        logger.Error($"コピー元フォルダ '{srcDirectory.FullName}' が見つかりませんでした。");
                        System.Windows.MessageBox.Show($"コピー元フォルダ '{srcDirectory.FullName}' が見つかりませんでした。", "コピー元フォルダ名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            return targetFiles.Distinct(new ImageFileComparer()).ToList();
        }

        private List<DirectoryInfo> GetDirectoriesToSeek(CopyCondition copyCondition, DirectoryInfo srcRootDir)
        {
            List<string> yyyymmddhhList = copyCondition.ToDaysList();
            List<DirectoryInfo> result = new List<DirectoryInfo>();
            foreach(string yyyymmddhh in yyyymmddhhList)
            {
                string yyyymmdd = yyyymmddhh.Substring(0, 8);
                string hh = yyyymmddhh.Substring(8, 2);
       
                DirectoryInfo dayDir = srcRootDir.GetDirectories(yyyymmdd).ToList().FirstOrDefault(null);
                if (dayDir == null)
                {
                    logger.Warn($"コピー元フォルダ {srcRootDir.FullName} の中に、コピー対象の日付フォルダ {yyyymmdd} が見つかりませんでした。");
                    continue;
                }
                DirectoryInfo hourDir = dayDir.GetDirectories(hh).ToList().FirstOrDefault(null);
                if (hourDir == null)
                {
                    logger.Warn($"コピー元フォルダ {dayDir.FullName} の中に、コピー対象の時刻フォルダ {hh} が見つかりませんでした。");
                    continue;
                }
                result.Add(hourDir);
            }
            return result;
        }
        
        public bool ExecuteCopy(List<FileInfo> filesToCopy, CopySetting copySetting)
        {
            bool doneSuccessfully = true;
            logger.Info($"ファイルコピーを開始します。");
            DirectoryInfo destRootDir = new DirectoryInfo(copySetting.destDir);
            if (!destRootDir.Exists)
            {
                logger.Error($"設定画面で入力されたコピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。");
                System.Windows.MessageBox.Show($"コピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。", "コピー先フォルダ名不正", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw new InvalidSettingsException($"コピー先フォルダ '{copySetting.destDir}' が見つかりませんでした。");
            }

            foreach(FileInfo f in filesToCopy)
            {
                try
                {
                    string yyyymmdd = f.CreationTime.ToString("yyyyMMdd");
                    string hh = f.CreationTime.ToString("hh");
                    DirectoryInfo dayDir = new DirectoryInfo(Path.Combine(destRootDir.FullName, yyyymmdd));
                    dayDir.Create();
                    DirectoryInfo hourDir = new DirectoryInfo(Path.Combine(dayDir.FullName, hh));
                    hourDir.Create();

                    f.CopyTo(Path.Combine(hourDir.FullName, f.Name), overwrite: true);
                    logger.Info($"ファイル '{f.Name}'を'{copySetting.srcDir}'から'{copySetting.destDir}'へコピーしました。");
                }
                catch (Exception ex)
                {
                    logger.Error($"ファイル {f.FullName} のコピーに失敗しました。エラー内容は以下を参照してください。");
                    logger.Error("**************************************************************************************************");
                    logger.Error(ex.ToString());
                    logger.Error("**************************************************************************************************");
                    throw ex;
                }
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
            if (from == null || to == null) return false;
            if (from > to) return false;
            return true;
        }

        public List<string> ToDaysList()
        {
            List<string> yyyymmddhhList = new List<string>();
            DateTime current = DateTimeUtil.RoundDownMinutes(this.from);
            while (current <= this.to)
            {
                yyyymmddhhList.Add(current.ToString("yyyyMMddHH"));
                current = current.AddHours(1);
            }
            return yyyymmddhhList;
        }
    }
}