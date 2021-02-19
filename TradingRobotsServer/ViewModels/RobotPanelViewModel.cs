using QuikSharp;
using System.Windows.Input;
using TestQuotes.Infrastructure.Commands;
using TestQuotes.ViewModels.Base;
using TradingRobotsServer.Models;

namespace TradingRobotsServer.ViewModels
{
    public class RobotPanelViewModel : ViewModel
    {
        #region - Переменные

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

        private string log;
        public string Log
        {
            get => log;
            set
            {
                log = value;
                OnPropertyChanged("Log");
            }
        }

        #endregion

        #region - Команды

        public ICommand Button1 { get; }
        public ICommand Button2 { get; }
        public RobotPanelViewModel()
        {
            Button1 = new RelayCommand(On_Button_Execute, Can_Button_Execute);
            Button2 = new RelayCommand(On_Button2_Execute, Can_Button2_Execute);
        }

        #endregion

        #region - Локальные переменные

        private TradingRobot robot;
        string param;
        #endregion

        #region - Обработчики событий формы

        private bool Can_Button_Execute(object obj)
        {
            return true;
        }
        private void On_Button_Execute(object obj)
        {
            //TradingRobot robotRIH1 = new TradingRobot();
            //param = "15;5;20;10,39,0;18,30,0;true;false";
            //robotRIH1.Run(Quik.DefaultPort, Quik.DefaultHost, "RIH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            //TradingRobot robotSiH1 = new TradingRobot();
            //param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotSiH1.Run(Quik.DefaultPort, Quik.DefaultHost, "SiH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            TradingRobot robotSRH1 = new TradingRobot();
            param = "15;5;2;10,39,0;18,30,0;true;false";
            robotSRH1.Run(Quik.DefaultPort, Quik.DefaultHost, "SiH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            //TradingRobot robotVBH1 = new TradingRobot();
            // param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotVBH1.Run(Quik.DefaultPort, Quik.DefaultHost, "VBH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            //TradingRobot robotGZH1 = new TradingRobot();
            //param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotGZH1.Run(Quik.DefaultPort, Quik.DefaultHost, "GZH1", QuikSharp.DataStructures.CandleInterval.M1, param);

        }

        private bool Can_Button2_Execute(object obj)
        {
            return true;
        }
        private void On_Button2_Execute(object obj)
        {
            //quik_connect.LimitOrder(0, Tools, QuikSharp.DataStructures.Operation.Buy, LastPrice + 2, 1);
            //quik_connect.LimitOrder(1, Tools, QuikSharp.DataStructures.Operation.Buy, LastPrice + 2, 1);

            //quik_connect.MarketOrder(0, Tools, QuikSharp.DataStructures.Operation.Buy, 1);
            //quik_connect.MarketOrder(1, Tools, QuikSharp.DataStructures.Operation.Buy, 1);

            //quik_connect.TakeProfitStotLimitOrder(Tools[0], 1.5m, 0.1m, LastPrice + 3, LastPrice - 2, LastPrice - 2.5m, QuikSharp.DataStructures.Operation.Sell, 2);
            //quik_connect.TakeProfitStotLimitOrder(Tools[1], 1.5m, 0.1m, LastPrice + 3, LastPrice - 2, LastPrice - 2.5m, QuikSharp.DataStructures.Operation.Sell, 2);
            //quik_connect.TakeProfitStotLimitOrder(Tools[0], 1.5m, 0.1m, LastPrice - 3, LastPrice + 2, LastPrice + 2.5m, QuikSharp.DataStructures.Operation.Sell, 2);
            //quik_connect.TakeProfitStotLimitOrder(Tools[1], 1.5m, 0.1m, LastPrice - 3, LastPrice + 2, LastPrice + 2.5m, QuikSharp.DataStructures.Operation.Sell, 2);

            //quik_connect.TakeProfitOrder(Tools[0], 0, 0.1m, LastPrice + 1, LastPrice + 1, QuikSharp.DataStructures.Operation.Sell, 2);
        }


        #endregion
    }
}
