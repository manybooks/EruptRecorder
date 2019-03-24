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
using Microsoft.WindowsAPICodePack.Dialogs;
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
                this.TriggerFilePath.DataContext = viewModel.recordingSetting;
                this.CopySettings.ItemsSource = viewModel.copySettings;
                this.CopySettings.DataContext = viewModel.copySettings;
                this.LogOutputDir.DataContext = viewModel.loggingSetting;

                UpdateLogger();
                logger.Info("システムを起動しました。");

                // ジョブの起動
                StartJob();
            }
            catch(Exception ex)
            {
                logger.Error("起動時に予期せぬエラーが発生しました。");
                logger.Error(ex.Message);
                // 異常終了しても設定を失わないように
                FinalizeProcess();
                this.Close();
            }
        }

        public void StartJob()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMinutes(viewModel.recordingSetting.intervalMinutesToDetect)
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
            timer.Interval = TimeSpan.FromMinutes(viewModel.recordingSetting.intervalMinutesToDetect);
        }

        public void ExecuteCopy()
        {
            UpdateLogger();
            ReadTrigerJob readTrigerJob = new ReadTrigerJob(viewModel.recordingSetting.triggerFilePath, logger);
            List<Models.EventTrigger> eventTriggers = readTrigerJob.Run(viewModel.recordingSetting.timeOfLastRun);

            List<bool> jobResults = new List<bool>();
            DateTime startCopyJobAt = DateTime.Now;
            foreach(CopySetting copySetting in viewModel.copySettings)
            {
                CopyJob copyJob = new CopyJob(logger);
                copyJob.Run(eventTriggers, copySetting, viewModel.recordingSetting, logger);
            }

            // 最終検出時刻を更新
            viewModel.recordingSetting.timeOfLastRun = startCopyJobAt;
        }

        public void UpdateLogger()
        {
            // 最新のログ出力フォルダを反映
            logger = EruptLogging.CreateLogger("EruptRecorderLogger", viewModel?.loggingSetting?.logOutputDir);
        }

        public void OnClickOkButton(object sender, RoutedEventArgs e)
        {
            UpdateLogger();
            logger.Info("OKボタンがクリックされました");
            ExecuteCopy();
        }

        public void OnClickCancelButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("キャンセルボタンがクリックされました");
        }

        public void OnClosingWindow(object sender, CancelEventArgs e)
        {
            // 余裕があれば終了確認する
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
                logger.Info("各種設定情報を保存しました。");
            }
            catch (Exception ex)
            {
                logger.Error("各種設定情報の保存に失敗しました。");
                logger.Error(ex.Message);
            }
        }

        public void OnSrcDirColumn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 選択された行を特定
            TextBlock selected = sender as TextBlock;
            var selectedRow = selected.DataContext as CopySetting;

            var selectFromExplorer = MessageBox.Show("エクスプローラーからコピー元フォルダを選択しますか？", "編集方法選択", MessageBoxButton.YesNo, MessageBoxImage.Information);
            // エクスプローラーから選択する
            if (selectFromExplorer == MessageBoxResult.Yes)
            {
                var dialog = new CommonOpenFileDialog("コピー元フォルダ選択");

                // フォルダ選択モード。
                dialog.IsFolderPicker = true;
                var ret = dialog.ShowDialog();
                if (ret != CommonFileDialogResult.Ok)
                {
                    return;
                }

                selectedRow.srcDir = dialog.FileName;
            }
            // テキストで入力する
            else
            {
                string inputPath = Microsoft.VisualBasic.Interaction.InputBox("コピー元フォルダを設定してください。", "テキスト入力");
                if(!string.IsNullOrEmpty(inputPath))
                {
                    selectedRow.srcDir = inputPath;
                }
            }
        }

        public void OnDestDirColumn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 選択された行を特定
            TextBlock selected = sender as TextBlock;
            var selectedRow = selected.DataContext as CopySetting;

            var selectFromExplorer = MessageBox.Show("エクスプローラーからコピー先フォルダを選択しますか？", "編集方法選択", MessageBoxButton.YesNo, MessageBoxImage.Information);
            // エクスプローラーから選択する
            if (selectFromExplorer == MessageBoxResult.Yes)
            {
                var dialog = new CommonOpenFileDialog("コピー先フォルダ選択");

                // フォルダ選択モード。
                dialog.IsFolderPicker = true;
                var ret = dialog.ShowDialog();
                if (ret != CommonFileDialogResult.Ok)
                {
                    return;
                }

                selectedRow.destDir = dialog.FileName;
            }
            // テキストで入力する
            else
            {
                string inputPath = Microsoft.VisualBasic.Interaction.InputBox("コピー先フォルダを設定してください。", "テキスト入力");
                if (!string.IsNullOrEmpty(inputPath))
                {
                    selectedRow.destDir = inputPath;
                }
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
            try
            {
                string settingFilePath = GetSettingFilePath();
                string settingJson = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<SettingsViewModel>(settingJson);
            }
            catch(FileNotFoundException)
            {
                UpdateLogger();
                logger.Error("設定情報を格納したファイルが見つかりませんでした。システム開発者に連絡してください。");
                return SettingsViewModel.DefaultSettings();
            }
        }

        public void SaveSettings()
        {
            if (!this.viewModel.IsValid()) return;
            string settingFilePath = GetSettingFilePath();
            File.WriteAllText(settingFilePath, JsonConvert.SerializeObject(viewModel));
        }

    }
}
