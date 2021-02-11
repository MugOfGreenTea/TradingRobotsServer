using System;
using System.Threading;
using Timer = System.Timers.Timer;
using QuikSharp.DataStructures;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Logic;
using TradingRobotsServer.Models.Logic.Base;
using System.Diagnostics;

namespace TradingRobotsServer.Models
{
    public class TradingRobot
    {
        public QuikConnect quik_connect;
        public Tool Tool;
        public Bot Bot;
        public Thread Thread;
        public Timer Timer;
        public Strategy Strategy;

        #region - Проверки

        bool check_quik_connecting; //проверка подключения к Quik 
        bool check_subscribe_tool_candles; //проверка подключения к инструменту
        bool check_subscribe_candles; //проверка подписки на свечи
        bool check_subscribe_orderbook; //проверка подписки на стакан
        bool check_subscribe_futures_client_holding; //проверка подписки на событие изменения позиции на срочном рынке
        bool check_subscribe_depo_limit; //проверка подписки на изменение лимита по бумагам
        bool check_subscribe_stoplimit; //проверка подписки на получение изменений позиции в стоп-заявках
        bool check_subscribe_trade;//проверка подписки на получение информации о новой сделке

        #endregion

        public void Run(int port, string host, string tool_name, CandleInterval candle_interval, string param)
        {
            InitialQuik();
            Connect(port, host, tool_name, candle_interval);
            RunStrategy(param);
        }

        private void InitialQuik()
        {
            quik_connect = new QuikConnect();
        }

        private void Connect(int port, string host, string tool_name, CandleInterval candle_interval)
        {
            check_quik_connecting = quik_connect.QuikConnecting(port, host);
            if (check_quik_connecting)
            {
                Tool = quik_connect.ToolConnectingReturn(tool_name, candle_interval);
            }

            if (Tool != null)
            {
                check_subscribe_orderbook = quik_connect.SubscribeOrderBook(ref Tool);
                check_subscribe_tool_candles = quik_connect.SubscribeToolReceiveCandles(ref Tool, candle_interval);
                check_subscribe_candles = quik_connect.SubscribeReceiveCandles();
                check_subscribe_futures_client_holding = quik_connect.SubscribeOnFuturesClientHolding();
                check_subscribe_depo_limit = quik_connect.SubscribeOnDepoLimit();

                Tool.SubscribeNewCandle();//подписка Tool на новые свечи от QuikConnect
            }
        }

        private void RunStrategy(string param)
        {
            if (check_subscribe_tool_candles && check_subscribe_candles && check_subscribe_orderbook
                    && check_subscribe_futures_client_holding && check_subscribe_depo_limit/* && check_subscribe_stoplimit*/)
            {
                Thread = new Thread(new ThreadStart(StartTimer));
                Thread.Start();
            }

            Strategy = new SetPositionByCandleHighLowStrategy(param, Bot);
            Bot = new Bot(quik_connect, Tool, Strategy);

            Strategy.Bot = Bot;
            Strategy.SubsribeNewDeal();
            Bot.SubsribeNewDataTool();//подписка Bot на новые свечи от Tool
            Bot.SubsribeNewOrderStrategy();//подписка Bot на новые заявки от Strategy
            Bot.SubsribeOnTrade();
            Bot.SubsribeOnOrder();
            Bot.SubscribeOnStopOrder();
        }

        private void StartTimer()
        {
            Timer = new Timer();
            Timer.Interval = 10000;
            Timer.Elapsed += CallLastPrice;
            Timer.Start();
        }

        private void CallLastPrice(object sender, EventArgs e)
        {
            Tool.CallLastPrice();
        }
    }
}
