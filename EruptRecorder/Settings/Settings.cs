using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EruptRecorder.Settings
{
    public class SettingsViewModel
    {
        public ObservableCollection<CopySetting> copySettings { get; set; }
        public RecordingSetting recordingSetting { get; set; }
        public LoggingSetting loggingSetting { get; set; }

        public bool IsValid()
        {
            // TODO
            return true;
        }


        public SettingsViewModel()
        {
        }
    }

    public class CopySetting
    {
        public bool isActive { get; set; }
        public int index { get; set; }
        public string srcDir { get; set; }
        public string destDir { get; set; }
        public string prefix { get; set; }
        public string fileExtension { get; set; }

        public CopySetting(bool isActive, int index, string srcDir, string destDir, string prefix, string fileExtension)
        {
            this.isActive = isActive;
            this.index = index;
            this.srcDir = srcDir;
            this.destDir = destDir;
            this.prefix = prefix;
            this.fileExtension = fileExtension;
        }
    }

    public class RecordingSetting
    {
        public int minutesToGoBack { get; set; } = 1;
        public int intervalMinutesToDetect { get; set; } = 1;
        public DateTime timeOfLastRun { get; set; }

        public RecordingSetting(int minutesToGoBack)
        {
            this.minutesToGoBack = minutesToGoBack;
        }
    }

    public class LoggingSetting
    {
        public string logOutputDir { get; set; }

        public LoggingSetting(string logOutputDir)
        {
            this.logOutputDir = logOutputDir;
        }
    }
}
