using LiveCharts;
using LiveCharts.Wpf;
using NLog;
using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TestQuotes.Infrastructure.Commands;
using TestQuotes.ViewModels.Base;
using TradingRobotsServer.Models;

namespace TradingRobotsServer.ViewModels
{
    public class TabStrategyViewModel : ViewModel
    {
        #region Информация об инструменте

        private string firm_id;
        public string FirmID
        {
            get => firm_id;
            set
            {
                firm_id = value;
                OnPropertyChanged("FirmID");
            }
        }

        private string code_client;
        public string CodeClient
        {
            get => code_client;
            set
            {
                code_client = value;
                OnPropertyChanged("CodeClient");
            }
        }

        private int account_id;
        public int AccountID
        {
            get => account_id;
            set
            {
                account_id = value;
                OnPropertyChanged("AccountID");
            }
        }

        private string name_tool;
        public string NameTool
        {
            get => name_tool;
            set
            {
                name_tool = value;
                OnPropertyChanged("NameTool");
            }
        }

        private string code_tool;
        public string CodeTool
        {
            get => code_tool;
            set
            {
                code_tool = value;
                OnPropertyChanged("CodeTool");
            }
        }

        private string code_class;
        public string CodeClass
        {
            get => code_class;
            set
            {
                code_class = value;
                OnPropertyChanged("CodeClass");
            }
        }

        private CandleInterval interval;
        public CandleInterval Interval //сделать нормальный вывод и присвоение интервала
        {
            get => interval;
            set
            {
                interval = value;
                OnPropertyChanged("Interval");
            }
        }

        private decimal step_price;
        public decimal StepPrice
        {
            get => step_price;
            set
            {
                step_price = value;
                OnPropertyChanged("StepPrice");
            }
        }

        private int lot;
        public int Lot
        {
            get => lot;
            set
            {
                lot = value;
                OnPropertyChanged("Lot");
            }
        }

        private decimal go_middle;
        public decimal GOMiddle
        {
            get => go_middle;
            set
            {
                go_middle = value;
                OnPropertyChanged("GOMiddle");
            }
        }

        private decimal var_margin;
        public decimal VarMargin
        {
            get => var_margin;
            set
            {
                var_margin = value;
                OnPropertyChanged("VarMargin");
            }
        }

        private decimal last_price;
        public decimal LastPrice
        {
            get => last_price;
            set
            {
                last_price = value;
                OnPropertyChanged("LastPrice");
            }
        }

        private bool status_connect;
        public bool StatusConnect
        {
            get => status_connect;
            set
            {
                status_connect = value;
                if (value)
                {
                    BrushesConnect = Brushes.Green;
                }
                else
                {
                    BrushesConnect = Brushes.Red;
                }
            }
        }

        public Brush BrushesConnect { get; set; }

        #endregion Информация об инструменте

        #region Свойства логирования

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string log;
        public string Log
        {
            get => log;
            set
            {
                log += value + "\r\n";
                logger.Info(value);
                OnPropertyChanged("Log");
            }
        }

        #endregion Свойства логирования

        #region Свойства графика

        private SeriesCollection series_candle;
        public SeriesCollection SeriesCandle
        {
            get => series_candle;
            set
            {
                series_candle = value;
                OnPropertyChanged("SeriesCandle");
            }
        }

        string[] labels;
        public string[] Labels
        {
            get { return labels; }
            set
            {
                labels = value;
                OnPropertyChanged("Labels");
            }
        }

        #endregion Свойства графика

        #region Свойства робота

        public TradingRobot Robot { get; set; }

        #endregion

        #region Параметры стратегии

        private string window_candle;
        public string WindowCandle
        {
            get => window_candle;
            set
            {
                window_candle = value;
                OnPropertyChanged("WindowCandle");
            }
        }

        private string count_candle;
        public string CountCandle
        {
            get => count_candle;
            set
            {
                count_candle = value;
                OnPropertyChanged("CountCandle");
            }
        }

        private string indent;
        public string Indent
        {
            get => indent;
            set
            {
                indent = value;
                OnPropertyChanged("Indent");
            }
        }

        private string not_trading_time_morning;
        public string NotTradingTimeMorning
        {
            get => not_trading_time_morning;
            set
            {
                not_trading_time_morning = value;
                OnPropertyChanged("NotTradingTimeMorning");
            }
        }

        private string not_trading_time_night;
        public string NotTradingTimeNight
        {
            get => not_trading_time_night;
            set
            {
                not_trading_time_night = value;
                OnPropertyChanged("NotTradingTimeNight");
            }
        }

        private string look_long;
        public bool LookLong
        {
            get => Convert.ToBoolean(look_long);
            set
            {
                look_long = value.ToString();
                OnPropertyChanged("LookLong");
            }
        }

        private string lool_short;
        public bool LookShort
        {
            get => Convert.ToBoolean(lool_short);
            set
            {
                lool_short = value.ToString();
                OnPropertyChanged("LookShort");
            }
        }

        #endregion Параметры стратегии

        #region Команды

        public ICommand ConnectQuikCommand { get; }
        public ICommand ConnectToolCommand { get; }
        public ICommand SaveSettingStrategyCommand { get; }
        public ICommand StartBotCommand { get; }
        public ICommand StopBotCommand { get; }

        #endregion Команды

        #region Конструктор

        public TabStrategyViewModel()
        {
            ConnectQuikCommand = new RelayCommand(On_ConnectQuikCommand_Execute, Can_ConnectQuikCommand_Execute);
            ConnectToolCommand = new RelayCommand(On_ConnectToolCommand_Execute, Can_ConnectToolCommand_Execute);
            SaveSettingStrategyCommand = new RelayCommand(On_SaveSettingStrategyCommand_Execute, Can_SaveSettingStrategyCommand_Execute);
            StartBotCommand = new RelayCommand(On_StartBotCommand_Execute, Can_StartBotCommand_Execute);
            StopBotCommand = new RelayCommand(On_StopBotCommand_Execute, Can_StopBotCommand_Execute);

            Debug.WriteLine("check");

            Robot = new TradingRobot();
        }

        #endregion Конструктор

        #region Обработчики событий

        private bool Can_ConnectQuikCommand_Execute(object obj)
        {
            return true;
        }
        private void On_ConnectQuikCommand_Execute(object obj)
        {
            Robot.ConnectQuik(Quik.DefaultPort, Quik.DefaultHost);
        }

        private bool Can_ConnectToolCommand_Execute(object obj)
        {
            return true;
        }
        private void On_ConnectToolCommand_Execute(object obj)
        {
            Robot.ConnectTool(CodeTool, CandleInterval.M1);
        }

        private bool Can_SaveSettingStrategyCommand_Execute(object obj)
        {
            return true;
        }
        private void On_SaveSettingStrategyCommand_Execute(object obj)
        {

        }

        private bool Can_StartBotCommand_Execute(object obj)
        {
            return true;
        }
        private void On_StartBotCommand_Execute(object obj)
        {
            Robot.RunStrategy(GetParamStrategy());
        }

        private bool Can_StopBotCommand_Execute(object obj)
        {
            return true;
        }
        private void On_StopBotCommand_Execute(object obj)
        {

        }

        #endregion Обработчики событий

        #region Саппорт методы

        //сделать нормальный парс времени
        private string GetParamStrategy()
        {
            string param = window_candle + ";" + count_candle + ";" + indent + ";" + 
                not_trading_time_morning + ";" + not_trading_time_night + ";" + 
                look_long + ";" + lool_short;
            param = "15;5;2;10,39,0;18,30,0;true;false";

            return param;
        }

        #endregion Саппорт методы
    }
}
