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
            if (trigers == null || trigers?.Count() == 0) return;

            List<EventTriger> trigersToCheck = trigers.Where(triger => triger.timeStamp > recordingSetting.timeOfLastRun)
                                                      .OrderBy(triger => triger.timeStamp)
                                                      .ToList();
            if (trigersToCheck?.Count() == 0) return;

            List<CopyCondition> copyConditions = new List<CopyCondition>();
            foreach (EventTriger triger in trigersToCheck)
            {
                if (triger.flag != copySetting.index) continue;

                CopyCondition copyCondition = new CopyCondition()
                {
                    from = triger.timeStamp.AddMinutes(-1 * recordingSetting.minutesToGoBack)
                };
            }
        }

        public List<CopyCondition> MergeTrigers(List<EventTriger> eventTrigers)
        {
            return new List<CopyCondition>();
        }

    }

    public class CopyCondition
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
    }
}