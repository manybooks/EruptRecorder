using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EruptRecorder.Settings;
using EruptRecorder.Logging;
using EruptRecorder.Jobs;
using EruptRecorder.Models;
using Newtonsoft.Json;
using log4net;

namespace EruptRecorder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ILog logger;
        DispatcherTimer timer;
        private const string SETTING_FILE_NAME = "settings.json";
        private const string TRIGGER_FILE_NAME = "trigger.csv";
        public SettingsViewModel ActiveViewModel;
        public SettingsViewModel BindingViewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                ActiveViewModel = LoadSettings();
                BindingViewModel = LoadSettings();

                this.MinutesToGoBack.DataContext = BindingViewModel.recordingSetting;
                this.IntervalMinutesToDetect.DataContext = BindingViewModel.recordingSetting;
                this.TimeOfLastRun.DataContext = BindingViewModel.recordingSetting;
                this.CopySettings.ItemsSource = BindingViewModel.copySettings;
                this.CopySettings.DataContext = BindingViewModel.copySettings;
                this.LogOutputDir.DataContext = BindingViewModel.loggingSetting;

                UpdateLogger();
                logger.Info("システムを起動しました。");

                // ジョブの起動
                StartJob();
            }
            finally
            {
                // 異常終了しても設定を失わないように
                FinalizeProcess();
            }
        }

        public void StartJob()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMinutes(ActiveViewModel.recordingSetting.intervalMinutesToDetect)
            };

            timer.Tick += (s, e) =>
            {
                logger.Info("ジョブを開始します。");
                ExecuteCopy();
                UpdateInterval();
            };

            timer.Start();

            this.Closing += (s, e) => timer.Stop();
        }

        public void UpdateInterval()
        {
            timer.Interval = TimeSpan.FromMinutes(ActiveViewModel.recordingSetting.intervalMinutesToDetect);
        }

        public void ExecuteCopy()
        {
            UpdateLogger();
            ReadTrigerJob readTrigerJob = new ReadTrigerJob(System.IO.Path.Combine(GetProjectRootDir().FullName, TRIGGER_FILE_NAME), logger);
            List<Models.EventTrigger> eventTriggers = readTrigerJob.Run(ActiveViewModel.recordingSetting.timeOfLastRun);

            List<bool> jobResults = new List<bool>();
            DateTime startCopyJobAt = DateTime.Now;
            foreach(CopySetting copySetting in ActiveViewModel.copySettings)
            {
                CopyJob copyJob = new CopyJob(logger);
                copyJob.Run(eventTriggers, copySetting, ActiveViewModel.recordingSetting, logger);
            }

            // 最終検出時刻を更新
            ActiveViewModel.recordingSetting.timeOfLastRun = startCopyJobAt;
            BindingViewModel.recordingSetting.timeOfLastRun = startCopyJobAt;
        }

        public void UpdateLogger()
        {
            // 最新のログ出力フォルダを反映
            logger = EruptLogging.CreateLogger("EruptRecorderLogger", ActiveViewModel.loggingSetting.logOutputDir);
        }

        public void OnClickOkButton(object sender, RoutedEventArgs e)
        {
            // TODO あとで消す
            logger.Info("OKボタンがクリックされました");
            ActiveViewModel = BindingViewModel;
        }

        public void OnClickCancelButton(object sender, RoutedEventArgs e)
        {
            // TODO あとで消す
            Console.WriteLine("キャンセルボタンがクリックされました");
            BindingViewModel = ActiveViewModel;
        }

        public void OnClosingWindow(object sender, CancelEventArgs e)
        {
            // TODO 余裕があれば終了確認をする
        }

        public void OnClosed(object sender, EventArgs e)
        {
            FinalizeProcess();
        }

        public void FinalizeProcess()
        {
            try
            {
                SaveSettings();
                logger.Info("システムを正常に終了しました。");
            }
            catch (Exception ex)
            {
                logger.Error("各種設定情報の保存に失敗しました。");
                logger.Error(ex.Message);
            }
        }

        private DirectoryInfo GetProjectRootDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            var projectRootDir = Directory.GetParent(currentDir).Parent.Parent;
            return projectRootDir;
        }

        private string GetSettingFilePath()
        {
            var projectRootDir = GetProjectRootDir();
            var settingFilePath = System.IO.Path.Combine(projectRootDir.FullName, SETTING_FILE_NAME);
            return settingFilePath;
        }

        public SettingsViewModel LoadSettings()
        {
            string settingFilePath = GetSettingFilePath();
            string settingJson = File.ReadAllText(settingFilePath);
            return JsonConvert.DeserializeObject<SettingsViewModel>(settingJson);
        }

        public void SaveSettings()
        {
            if (!this.ActiveViewModel.IsValid()) return;
            string settingFilePath = GetSettingFilePath();
            File.WriteAllText(settingFilePath, JsonConvert.SerializeObject(ActiveViewModel));
        }
    }
}
