using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private const string SETTING_FILE_NAME = "settings.json";
        private const string TRIGGER_FILE_NAME = "trigger.csv";
        public SettingsViewModel viewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                viewModel = LoadSettings();
                this.MinutesToGoBack.DataContext = viewModel.recordingSetting;
                this.IntervalMinutesToDetect.DataContext = viewModel.recordingSetting;
                this.TimeOfLastRun.DataContext = viewModel.recordingSetting;
                this.CopySettings.ItemsSource = viewModel.copySettings;
                this.LogOutputDir.DataContext = viewModel.loggingSetting;
            }
            finally
            {
                // 異常終了しても設定を失わないように
                SaveSettings();
            }
        }

        public void ExecuteCopy()
        {
            UpdateLogger();
            ReadTrigerJob readTrigerJob = new ReadTrigerJob(System.IO.Path.Combine(GetProjectRootDir().FullName, TRIGGER_FILE_NAME));
            List<Models.EventTrigger> eventTriggers = readTrigerJob.Run(viewModel.recordingSetting.timeOfLastRun);
            foreach(CopySetting copySetting in viewModel.copySettings)
            {
                CopyJob copyJob = new CopyJob(logger);
                copyJob.Run(eventTriggers, copySetting, viewModel.recordingSetting, logger);
            }
        }

        public void UpdateLogger()
        {
            // 最新のログ出力フォルダを反映
            logger = EruptLogging.CreateLogger("EruptRecorderLogger", viewModel.loggingSetting.logOutputDir);
        }

        public void OnClickOkButton(object sender, RoutedEventArgs e)
        {
            UpdateLogger();
            logger.Info("OKボタンがクリックされました");
        }

        public void OnClickCancelButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("キャンセルボタンがクリックされました");
        }

        public void OnClosingWindow(object sender, CancelEventArgs e)
        {
            SaveSettings();
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
            if (!this.viewModel.IsValid()) return;
            string settingFilePath = GetSettingFilePath();
            File.WriteAllText(settingFilePath, JsonConvert.SerializeObject(viewModel));
        }
    }
}
