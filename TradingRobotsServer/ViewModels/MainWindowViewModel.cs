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

namespace TradingRobotsServer.ViewModels
{
    public class MainWindowViewModel : ViewModel
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
        #endregion

        #region

        public ICommand Button { get; }

        public MainWindowViewModel()
        {
            Button = new RelayCommand(On_Button_Execute, Can_Button_Execute);
        }

        #endregion

        #region

        public QuikConnect quik_connect;
        public Thread thread;
        public System.Timers.Timer timer;
        bool check_quik_connecting;
        bool check_tool_connecting;
        bool check_subscribe_candles;
        bool check_subscribe_orderbook;
        bool check_subscribe_futures_client_holding;
        bool check_subscribe_depo_limit;
        #endregion

        #region

        private bool Can_Button_Execute(object obj)
        {
            return true;
        }
        private void On_Button_Execute(object obj)
        {
            quik_connect = new QuikConnect(this);
            check_quik_connecting = quik_connect.QuikConnecting(Quik.DefaultPort, Quik.DefaultHost);
            if (check_quik_connecting)
                check_tool_connecting = quik_connect.ToolConnecting("RIH1", QuikSharp.DataStructures.CandleInterval.M1);

            if (check_tool_connecting)
            {
                check_subscribe_candles = quik_connect.SubscribeReceiveCandles(0, QuikSharp.DataStructures.CandleInterval.M1);
                check_subscribe_orderbook = quik_connect.SubscribeOrderBook(0);
                check_subscribe_futures_client_holding = quik_connect.SubscribeOnFuturesClientHolding();
                check_subscribe_depo_limit = quik_connect.SubscribeOnDepoLimit();

                if (check_subscribe_candles && check_subscribe_orderbook && check_subscribe_futures_client_holding && check_subscribe_depo_limit)
                {
                    thread = new Thread(new ThreadStart(StartTimer));
                    thread.Start();
                }
            }
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
            LastPrice = quik_connect.Tools[0].LastPrice;
        }

        #endregion
    }
}
