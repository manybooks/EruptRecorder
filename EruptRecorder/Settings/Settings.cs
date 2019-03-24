using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static SettingsViewModel DefaultSettings()
        {
            return new SettingsViewModel()
            {
                copySettings = new ObservableCollection<CopySetting>()
                {
                    new CopySetting()
                    {
                        isActive = false,
                        index = 1,
                        fileExtension = "bmp",
                        prefix = "",
                        srcDir = "",
                        destDir = ""
                    }
                },
                recordingSetting = new RecordingSetting()
                {
                    minutesToGoBack = 10,
                    intervalMinutesToDetect = 1,
                    timeOfLastRun = new DateTime(),
                    triggerFilePath = "echoindex.dat"
                },
                loggingSetting = new LoggingSetting()
                {
                    logOutputDir = ""
                }
            };
        }
    }

    public class CopySetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool isActive { get; set; }
        public int index { get; set; }
        private string _srcDir;
        public string srcDir
        {
            get
            {
                return this._srcDir;
            }
            set
            {
                if (value != this._srcDir)
                {
                    this._srcDir = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string _destDir;
        public string destDir
        {
            get
            {
                return this._destDir;
            }
            set
            {
                if (value != this._destDir)
                {
                    this._destDir = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string prefix { get; set; }
        public string fileExtension { get; set; }

        public CopySetting()
        {
        }
    }

    public class RecordingSetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int minutesToGoBack { get; set; } = 1;
        public int intervalMinutesToDetect { get; set; } = 1;
        public string triggerFilePath { get; set; }
        private DateTime? _timeOfLastRun;
        public DateTime? timeOfLastRun
        {
            get
            {
                return this._timeOfLastRun;
            }
            set
            {
                if(value != this._timeOfLastRun)
                {
                    this._timeOfLastRun = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RecordingSetting()
        {
        }
    }

    public class LoggingSetting
    {
        public string logOutputDir { get; set; }

        public LoggingSetting()
        {
        }
    }
}
