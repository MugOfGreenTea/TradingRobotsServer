using QuikSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using TestQuotes.Infrastructure.Commands;
using TestQuotes.ViewModels.Base;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Strategy;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.ViewModels
{
    public class RobotPanelViewModel : ViewModel
    {
        #region

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

        private decimal sale_price;
        public decimal SalePrice
        {
            get => sale_price;
            set
            {
                sale_price = value;
                OnPropertyChanged("SalePrice");
            }
        }

        #endregion

        #region

        public ICommand Button1 { get; }
        public ICommand Button2 { get; }
        public RobotPanelViewModel()
        {
            Button1 = new RelayCommand(On_Button_Execute, Can_Button_Execute);
            Button2 = new RelayCommand(On_Button2_Execute, Can_Button2_Execute);
        }

        #endregion

        #region

        public QuikConnect quik_connect;
        public Tools Tools;
        public Thread thread;
        public System.Timers.Timer timer;
        bool check_quik_connecting;
        bool check_tool_connecting;
        bool check_subscribe_candles;
        bool check_subscribe_orderbook;
        bool check_subscribe_futures_client_holding;
        bool check_subscribe_depo_limit;
        bool check_subscribe_stoplimit;

        SetPositionByCandleHighLowStrategy strategy;

        #endregion

        #region

        private bool Can_Button_Execute(object obj)
        {
            return true;
        }
        private void On_Button_Execute(object obj)
        {
            quik_connect = new QuikConnect(this);
            Tools = new Tools();
            check_quik_connecting = quik_connect.QuikConnecting(Quik.DefaultPort, Quik.DefaultHost);
            if (check_quik_connecting)
            {
                Tools.Add(quik_connect.ToolConnectingReturn("SBER", QuikSharp.DataStructures.CandleInterval.M1));
                Tools.Add(quik_connect.ToolConnectingReturn("GAZP", QuikSharp.DataStructures.CandleInterval.M1));
                //check_tool_connecting = quik_connect.ToolConnecting("GAZP", QuikSharp.DataStructures.CandleInterval.M1);
            }

            //if (check_tool_connecting)
            {
                check_subscribe_candles = quik_connect.SubscribeReceiveCandles(ref Tools, 0, QuikSharp.DataStructures.CandleInterval.M1);
                check_subscribe_orderbook = quik_connect.SubscribeOrderBook(ref Tools, 0);
                check_subscribe_candles = quik_connect.SubscribeReceiveCandles(ref Tools, 1, QuikSharp.DataStructures.CandleInterval.M1);
                check_subscribe_orderbook = quik_connect.SubscribeOrderBook(ref Tools, 1);
                check_subscribe_futures_client_holding = quik_connect.SubscribeOnFuturesClientHolding();
                check_subscribe_depo_limit = quik_connect.SubscribeOnDepoLimit();
                check_subscribe_stoplimit = quik_connect.SubscribeOnStopOrder();

                if (check_subscribe_candles && check_subscribe_orderbook && check_subscribe_futures_client_holding
                    && check_subscribe_depo_limit && check_subscribe_stoplimit)
                {
                    thread = new Thread(new ThreadStart(StartTimer));
                    thread.Start();
                }
            }

            strategy = new SetPositionByCandleHighLowStrategy(15, 5, 0.3m, new TimeSpan(10, 39, 0));
        }

        private void StartTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += CallLastPrice;
            timer.Start();
        }

        private void CallLastPrice(object sender, EventArgs e)
        {
            LastPrice = Tools[0].LastPrice;
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

            quik_connect.TakeProfitOrder(Tools[0], 0, 0.1m, LastPrice + 1, LastPrice + 1, QuikSharp.DataStructures.Operation.Sell, 2);
        }


        #endregion
    }
}
