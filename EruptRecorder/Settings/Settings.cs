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


        public SettingsViewModel()
        {
            /*
            // TODO: デバッグ用コード。あとで消す。
            this.copySettings = new ObservableCollection<CopySetting>
            {
                new CopySetting(true, 1, "srcDir1", "destDir1", "aaa", ".img"),
                new CopySetting(true, 2, "srcDir2", "destDir2", "bbb", ".jpg"),
                new CopySetting(false, 3, "srcDir3", "destDir3", "ccc", ".*"),
                new CopySetting(true, 4, "srcDir4", "destDir4", "ddd", ".txt"),
            };
            this.recordingSetting = new RecordingSetting(5);
            this.loggingSetting = new LoggingSetting("log/");
            */
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
        public int minutesToGoBack { get; set; }
        public int runPerMinutes { get; set; }
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
