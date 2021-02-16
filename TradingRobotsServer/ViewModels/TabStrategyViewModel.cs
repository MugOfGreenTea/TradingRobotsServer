using LiveCharts;
using LiveCharts.Wpf;
using NLog;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TestQuotes.ViewModels.Base;

namespace TradingRobotsServer.ViewModels
{
    class TabStrategyViewModel : ViewModel
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

        public Brush BrushesConnect;

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

        #region Команды

        public ICommand ConnectQuikAndToolCommand { get; }
        public ICommand SaveSettingStrategyCommand { get; }
        public ICommand StartBotCommand { get; }
        public ICommand StopBotCommand { get; }

        #endregion Команды

        #region Конструктор

        public TabStrategyViewModel()
        {

        }

        #endregion Конструктор
    }
}
