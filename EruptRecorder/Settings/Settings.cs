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
using System.Windows.Media;

namespace EruptRecorder.Settings
{
    public class SettingsViewModel
    {
        public CopySetting copySetting { get; set; }
        public RecordingSetting recordingSetting { get; set; }
        public LoggingSetting loggingSetting { get; set; }
        public GlobalStatus globalStatus { get; set; }

        public bool IsValid()
        {
            if (!this.recordingSetting.IsValid()) return false;
            if (!this.loggingSetting.IsValid()) return false;
            if (!copySetting.IsValid()) return false;
            return true;
        }


        public SettingsViewModel()
        {
        }

        public static SettingsViewModel DefaultSettings()
        {
            return new SettingsViewModel()
            {
                copySetting = new CopySetting()
                {
                    copyStartDateTime = new DateTime(1900, 1, 1, 0, 0, 0),
                    copyEndDateTime = new DateTime(3000, 1, 1, 0, 0, 0),
                    srcDir = "",
                    destDir = ""
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
                },
                globalStatus = new GlobalStatus()
                {
                    status = GlobalStatus.AppStatus.NotReady
                }
            };
        }

        public void ReflectTheValueOf(SettingsViewModel another)
        {
            this.recordingSetting.intervalMinutesToDetect = another.recordingSetting.intervalMinutesToDetect;
            this.recordingSetting.minutesToGoBack = another.recordingSetting.minutesToGoBack;
            this.recordingSetting.triggerFilePath = another.recordingSetting.triggerFilePath;

            this.loggingSetting.logOutputDir = another.loggingSetting.logOutputDir;

            this.copySetting.copyStartDateTime = another.copySetting.copyStartDateTime;
            this.copySetting.copyEndDateTime = another.copySetting.copyEndDateTime;
            this.copySetting.srcDir = another.copySetting.srcDir;
            this.copySetting.destDir = another.copySetting.destDir;
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

        private DateTime _copyStartDateTime;
        public DateTime copyStartDateTime
        {
            get
            {
                return this._copyStartDateTime;
            }
            set
            {
                if (value != this._copyStartDateTime)
                {
                    this._copyStartDateTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private DateTime _copyEndDateTime;
        public DateTime copyEndDateTime
        {
            get
            {
                return this._copyEndDateTime;
            }
            set
            {
                if (value != this._copyEndDateTime)
                {
                    this._copyEndDateTime = value;
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
            if (this.copyStartDateTime == null) throw new InvalidSettingsException("コピー開始日時は入力必須です。");
            if (this.copyEndDateTime == null) throw new InvalidSettingsException("コピー終了日時は入力必須です。");
            if (this.copyStartDateTime > this.copyEndDateTime) throw new InvalidSettingsException("コピー開始日時とコピー終了日時の前後関係が不正です。");
            if (string.IsNullOrEmpty(this.srcDir)) throw new InvalidSettingsException("コピー元フォルダは入力必須です。");
            if (!Directory.Exists(this.srcDir)) throw new InvalidSettingsException($"指定されたコピー元フォルダが存在しません。");
            if (string.IsNullOrEmpty(this.destDir)) throw new InvalidSettingsException("コピー先フォルダは入力必須です。");
            if (!Directory.Exists(this.destDir)) throw new InvalidSettingsException($"指定されたコピー先フォルダが存在しません。");

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
            if (!File.Exists(this.triggerFilePath)) throw new InvalidSettingsException("指定されたトリガーファイルが存在しません。");
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
            if (string.IsNullOrEmpty(this.logOutputDir)) return true;
            if (!Directory.Exists(this.logOutputDir)) throw new InvalidSettingsException("指定されたログ保存先フォルダが存在しません。");
            return true;
        }
    }

    public class GlobalStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public enum AppStatus
        {
            Working,
            Pause,
            NotReady
        }

        private AppStatus _status { get; set; }
        public AppStatus status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value != this._status)
                {
                    this._status = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _description
        {
            get
            {
                switch (this.status)
                {
                    case AppStatus.Working:
                        return "正常作動中";
                    case AppStatus.Pause:
                        return "一時停止中";
                    case AppStatus.NotReady:
                        return "設定不備";
                    default:
                        return "設定不備";
                }
            }
        }

        public string description
        {
            get
            {
                return this._description;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        private Brush _descColor
        {
            get
            {
                switch (this.status)
                {
                    case AppStatus.Working:
                        return new SolidColorBrush(Colors.DarkGreen);
                    case AppStatus.Pause:
                        return new SolidColorBrush(Colors.Black);
                    case AppStatus.NotReady:
                        return new SolidColorBrush(Colors.Red);
                    default:
                        return new SolidColorBrush(Colors.Red);
                }

            }
        }
        public Brush descColor
        {
            get
            {
                return this._descColor;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        private string _buttonSymbol
        {
            get
            {
                switch (this.status)
                {
                    case AppStatus.Working:
                        return "Ⅱ";
                    case AppStatus.Pause:
                        return "▶";
                    case AppStatus.NotReady:
                        return "NG";
                    default:
                        return "NG";
                }
            }
        }
        public string buttonSymbol
        {
            get
            {
                return this._buttonSymbol;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }
    }
}
