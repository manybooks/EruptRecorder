using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EruptRecorder.Models;
using EruptRecorder.Settings;

namespace EruptRecorder.Jobs
{
    public class CopyJob
    {
        public void Run(List<EventTriger> trigers, CopySetting copySetting, RecordingSetting recordingSetting)
        {
            if (!copySetting.isActive) return;
            if (trigers == null || trigers?.Count() == 0)
            {
                logger.LogInfo("トリガーとなるCSVファイルの読み込みがうまく行かなかった可能性があります。");
                return;
            }

            List<EventTriger> trigersToCheck = trigers.Where(triger => triger.timeStamp > recordingSetting.timeOfLastRun)
                                                      .OrderBy(triger => triger.timeStamp)
                                                      .ToList();
            if (trigersToCheck?.Count() == 0)
            {
                return;
            }

            List<CopyCondition> copyConditions = GetCopyConditionsFrom(trigersToCheck, copySetting.index, recordingSetting.minutesToGoBack);
            if (copyConditions?.Count() == 0)
            {
                logger.LogInfo($"前回のジョブ起動から、Index '{copySetting.index}'のトリガーレコードは増えていませんでした。");
                return;
            }

            List<FileInfo> filesToCopy = GetCopyTargetFiles(copyConditions, copySetting);
            if (filesToCopy?.Count() == 0)
            {
                logger.LogWarn($"コピー元のディレクトリ{copySetting.srcDir}に条件を満たすファイルが見つかりませんでした。");
                return;
            }

            bool done = ExecuteCopy(filesToCopy, copySetting);
            if (!done)
            {
                logger.LogWarn("コピーに失敗しました。");
            }
        }

        public List<CopyCondition> MergeTrigers(List<EventTriger> eventTrigers)
        {
            return new List<CopyCondition>();
        }

        public List<CopyCondition> GetCopyConditionsFrom(List<EventTriger> eventTrigers, int targetIndex, int minutesToGoBack)
        {
            if (eventTrigers?.Count() < 1) throw new ArgumentException("対象となるトリガーがありませんでした。");

            List<CopyCondition> copyConditions = new List<CopyCondition>();
            CopyCondition currentCondition = new CopyCondition();
            DateTime currentTo = eventTrigers.FirstOrDefault().timeStamp;
            bool isActive = false;

            foreach (EventTriger triger in eventTrigers)
            {
                if (isActive)
                {
                    // Deactivate開始
                    if (triger.flag != targetIndex)
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
                    if (triger.flag == targetIndex)
                    {
                        isActive = true;
                        currentCondition.from = triger.timeStamp.AddMinutes(-1 * minutesToGoBack);
                    }
                }

                if (triger.flag == targetIndex)
                {
                    // Deactivate時に設定する遡り終了時刻を設定
                    currentTo = triger.timeStamp;
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