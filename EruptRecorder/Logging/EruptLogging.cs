using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace EruptRecorder.Logging
{
    public static class EruptLogging
    {
        public static ILog CreateLogger(string loggerName, string logOutputDir)
        {
            // Loggerの生成
            var logger = LogManager.GetLogger(loggerName);

            // RootのLoggerを取得
            var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;

            // RootのAppenderを取得
            var appender = rootLogger.GetAppender("DailyFileAppender") as FileAppender;

            // ファイル名の取得
            var filepath = appender.File;

            // ファイル名の設定
            appender.File = logOutputDir;
            appender.ActivateOptions();

            return logger;
        }
    }
}
