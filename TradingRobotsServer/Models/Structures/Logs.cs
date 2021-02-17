using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public static class Logs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void DebugLog(string log_string, LogType log_type)
        {
            Debug.WriteLine(log_string);
            //mainWindow.Log += log_string + "\r\n";

            switch (log_type)
            {
                case LogType.Null:
                    break;
                case LogType.Debug:
                    logger.Debug(log_string);
                    break;
                case LogType.Info:
                    logger.Info(log_string);
                    break;
                case LogType.Warn:
                    logger.Warn(log_string);
                    break;
                case LogType.Error:
                    logger.Error(log_string);
                    break;
                case LogType.Fatal:
                    logger.Fatal(log_string);
                    break;
                default:
                    break;
            }
        }
    }
}
