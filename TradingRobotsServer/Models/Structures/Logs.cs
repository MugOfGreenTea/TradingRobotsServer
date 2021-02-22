using ClosedXML.Excel;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public static void LogExcel(List<Deal> Deals)
        {
            string file_path = "logdeal.xlsx";
            XLWorkbook workbook = new XLWorkbook();

            XLWorkbook wbSource = new XLWorkbook(file_path);
            wbSource.Worksheet(1).CopyTo(workbook, "log");

            IXLWorksheet ws = workbook.Worksheet(1);

            int number_string = 2;

            foreach (Deal deal in Deals)
            {
                ws.Cell("a" + (number_string)).Value = deal.ID; // номер сделки
                ws.Cell("b" + (number_string)).Value = deal.Operation; // тип сделки

                if (deal.TradeEntryPoint != null)
                    ws.Cell("c" + (number_string)).Value = deal.TradeEntryPoint.DateTime; // время входа

                if (deal.TakeProfitOrdersInfo.Count > 0)
                    ws.Cell("D" + (number_string)).Value = deal.TakeProfitOrdersInfo[0].ExecutionLinkedStatus; // 1 тейк-профит
                else
                    ws.Cell("D" + (number_string)).Value = "Null"; // 1 тейк-профит

                if (deal.StopLimitOrdersInfo.Count > 0)
                    ws.Cell("E" + (number_string)).Value = deal.StopLimitOrdersInfo[0].ExecutionLinkedStatus; // 1 стоплосс
                else
                    ws.Cell("E" + (number_string)).Value = "Null";

                if (deal.TakeProfitOrdersInfo.Count > 1)
                    ws.Cell("F" + (number_string)).Value = deal.TakeProfitOrdersInfo[1].ExecutionLinkedStatus; // 2 тейк-профит
                else
                    ws.Cell("F" + (number_string)).Value = "Null";

                if (deal.StopLimitOrdersInfo.Count > 1)
                    ws.Cell("G" + (number_string)).Value = deal.StopLimitOrdersInfo[1].ExecutionLinkedStatus; // 2 стоплосс
                else
                    ws.Cell("G" + (number_string)).Value = "Null";

                if (deal.TakeProfitOrdersInfo.Count > 2)
                    ws.Cell("H" + (number_string)).Value = deal.TakeProfitOrdersInfo[2].ExecutionLinkedStatus; // 3 тейк-профит
                else
                    ws.Cell("H" + (number_string)).Value = "Null";

                if (deal.StopLimitOrdersInfo.Count > 2)
                    ws.Cell("I" + (number_string)).Value = deal.StopLimitOrdersInfo[2].ExecutionLinkedStatus; // 3 стоплосс
                else
                    ws.Cell("I" + (number_string)).Value = "Null";

                if (deal.ExitPoint != null)
                    ws.Cell("J" + (number_string)).Value = deal.ExitPoint.DateTime; // время выхода

                ws.Cell("K" + (number_string)).Value = deal.ReasonStop; // причина выхода

                foreach (string log in deal.Logs)
                    ws.Cell("L" + (number_string)).Value += log; // лог
                number_string++;
            }

            int i = 1;
            while (true)
            {
                string save_path = "logdeal-" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + i + ".xlsx";
                if (!File.Exists(save_path))
                {
                    workbook.SaveAs(save_path);
                    break;
                }
                i++;
            }
        }
    }
}
