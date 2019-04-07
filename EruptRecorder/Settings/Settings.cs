using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            if (!this.recordingSetting.IsValid()) return false;
            if (!this.loggingSetting.IsValid()) return false;
            foreach (CopySetting copySetting in copySettings)
            {
                if (!copySetting.IsValid()) return false;
            }
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
                    triggerFilePath = ""
                },
                loggingSetting = new LoggingSetting()
                {
                    logOutputDir = ""
                }
            };
        }

        public void ReflectTheValueOf(SettingsViewModel another)
        {
            this.recordingSetting.intervalMinutesToDetect = another.recordingSetting.intervalMinutesToDetect;
            this.recordingSetting.minutesToGoBack = another.recordingSetting.minutesToGoBack;
            this.recordingSetting.triggerFilePath = another.recordingSetting.triggerFilePath;

            this.loggingSetting.logOutputDir = another.loggingSetting.logOutputDir;

            this.copySettings = new ObservableCollection<CopySetting>();
            for (int i = 0; i < another.copySettings.Count(); i++)
            {
                CopySetting newOne = new CopySetting();
                this.copySettings.Add(newOne);

                this.copySettings[i].isActive = another.copySettings[i].isActive;
                this.copySettings[i].index = another.copySettings[i].index;
                this.copySettings[i].prefix = another.copySettings[i].prefix;
                this.copySettings[i].fileExtension = another.copySettings[i].fileExtension;
                this.copySettings[i].srcDir = another.copySettings[i].srcDir;
                this.copySettings[i].destDir = another.copySettings[i].destDir;
            }
        }
    }

    public class InvalidSettingsException : InvalidOperationException
    {
        public InvalidSettingsException(string message) : base(message)
        {
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

        public bool _isActive;
        public bool isActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int _index;
        public int index
        {
            get
            {
                return this._index;
            }
            set
            {
                if (value != this._index)
                {
                    this._index = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string _prefix;
        public string prefix
        {
            get
            {
                return this._prefix;
            }
            set
            {
                if (value != this._prefix)
                {
                    this._prefix = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string _fileExtension;
        public string fileExtension
        {
            get
            {
                return this._fileExtension;
            }
            set
            {
                if (value != this._fileExtension)
                {
                    this._fileExtension = value;
                    NotifyPropertyChanged();
                }
            }
        }
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

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(this.fileExtension)) throw new InvalidSettingsException("コピー設定のファイル拡張子は入力必須です。");
            if (this.fileExtension.StartsWith(".")) throw new InvalidSettingsException("コピー設定のファイル拡張子に'.'を含まないでください。");
            if (!AreEqual(Regex.Matches(this.fileExtension, "[a-z]+"), this.fileExtension) && this.fileExtension != "*") throw new InvalidSettingsException("コピー設定のファイル拡張子には小文字のアルファベットまたは'*'のみを使用して下さい。");
            if (string.IsNullOrEmpty(this.srcDir)) throw new InvalidSettingsException("コピー設定のコピー元フォルダは入力必須です。");
            if (string.IsNullOrEmpty(this.destDir)) throw new InvalidSettingsException("コピー設定のコピー先フォルダは入力必須です。");
            return true;
        }

        public static bool AreEqual(MatchCollection match, string origin)
        {
            string matched = "";
            foreach(Match m in match)
            {
                matched += m.Value;
            }
            return matched == origin;
        }

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

        public int _minutesToGoBack;
        public int minutesToGoBack
        {
            get
            {
                return this._minutesToGoBack;
            }
            set
            {
                if (value != this._minutesToGoBack)
                {
                    this._minutesToGoBack = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int _intervalMinutesToDetect;
        public int intervalMinutesToDetect
        {
            get
            {
                return this._intervalMinutesToDetect;
            }
            set
            {
                if (value != this._intervalMinutesToDetect)
                {
                    this._intervalMinutesToDetect = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string _triggerFilePath;
        public string triggerFilePath
        {
            get
            {
                return this._triggerFilePath;
            }
            set
            {
                if (value != this._triggerFilePath)
                {
                    this._triggerFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }
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

        public bool IsValid()
        {
            if (this.minutesToGoBack < 1) throw new InvalidSettingsException("さかのぼり時間には0以上の整数を設定してください。");
            if (this.intervalMinutesToDetect < 1) throw new InvalidSettingsException("検出インターバルには0以上の整数を設定してください。");
            if (string.IsNullOrEmpty(this.triggerFilePath)) throw new InvalidSettingsException("トリガーファイル名の入力は必須です。");
            return true;
        }
    }

    public class LoggingSetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string _logOutputDir;
        public string logOutputDir
        {
            get
            {
                return this._logOutputDir;
            }
            set
            {
                if (value != this._logOutputDir)
                {
                    this._logOutputDir = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public LoggingSetting()
        {
        }

        public bool IsValid()
        {
            return true;
        }
    }
}
