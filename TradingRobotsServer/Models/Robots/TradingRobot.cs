using QuikSharp.DataStructures;
using System;
using System.Diagnostics;
using System.Threading;
using TradingRobotsServer.Models.Logic;
using TradingRobotsServer.Models.Logic.Base;
using TradingRobotsServer.Models.QuikConnector;
using Timer = System.Timers.Timer;

namespace TradingRobotsServer.Models
{
    public class TradingRobot
    {
        public QuikConnect quik_connect;
        public Tool Tool;
        public Bot Bot;
        public Thread ThreadToolStrategy;
        public Timer TimerToolStrategy;

        public Strategy Strategy;

        private int Port;
        private string Host;
        private string ToolName;
        private CandleInterval Interval;
        private string Param;

        #region - Проверки

        bool check_quik_connecting; //проверка подключения к Quik 
        bool check_subscribe_tool_candles; //проверка подключения к инструменту
        bool check_subscribe_candles; //проверка подписки на свечи
        bool check_subscribe_orderbook; //проверка подписки на стакан
        bool check_subscribe_futures_client_holding; //проверка подписки на событие изменения позиции на срочном рынке
        bool check_subscribe_depo_limit; //проверка подписки на изменение лимита по бумагам
        bool check_subscribe_stoplimit; //проверка подписки на получение изменений позиции в стоп-заявках
        bool check_subscribe_trade;//проверка подписки на получение информации о новой сделке
        bool check_waiting_for_connection = false;
        bool check_waiting_for_tool = false;

        #endregion

        public void SetParam(int port, string host, string tool_name, CandleInterval candle_interval, string param)
        {
            Port = port;
            Host = host;
            ToolName = tool_name;
            Interval = candle_interval;
            Param = param;
        }

        public void ConnectQuik(int port, string host)
        {
            Port = port;
            Host = host;

            quik_connect = new QuikConnect();
            check_quik_connecting = quik_connect.QuikConnecting(Port, Host);
        }

        public void CallQuikConnect()
        {
            check_quik_connecting = quik_connect.CallQuikConnecting();
        }

        public void ConnectTool(string tool_name, CandleInterval candle_interval)
        {
            ToolName = tool_name;
            Interval = candle_interval;

            if (check_quik_connecting)
            {
                Tool = quik_connect.ToolConnectingReturn(ToolName, Interval);
            }

            if (Tool != null)
            {
                check_subscribe_tool_candles = quik_connect.SubscribeToolReceiveCandles(ref Tool, Interval);

                Tool.SubscribeNewCandle();//подписка Tool на новые свечи от QuikConnect
            }
        }

        public void RunStrategy(string param)
        {
            Param = param;
            if (check_subscribe_tool_candles && check_subscribe_orderbook)
            {
                ThreadToolStrategy = new Thread(new ThreadStart(StartTimerCallLastPrice));
                ThreadToolStrategy.Start();
            }

            Strategy = new SetPositionByCandleHighLowStrategy(Param, Bot);
            Bot = new Bot(quik_connect, Tool, Strategy);

            Strategy.Bot = Bot;
            Strategy.SubsribeNewDeal();
            Bot.SubsribeNewCandleToolAsync();//подписка Bot на новые свечи от Tool
            Bot.SubsribeNewOrderStrategy();//подписка Bot на новые заявки от Strategy
            Bot.SubsribeOnOrder();//подписка Bot на новые ордера
            Bot.SubscribeOnStopOrder();//подписка Bot на новые стопордера
            Bot.SubsribeOnTrade();

            int i = 5;
            Debug.WriteLine(++i + ++i);
        }

        private void StartTimerCallLastPrice()
        {
            TimerToolStrategy = new Timer();
            TimerToolStrategy.Interval = 5000;
            TimerToolStrategy.Elapsed += CallLastPrice;
            TimerToolStrategy.Start();
        }

        private void CallLastPrice(object sender, EventArgs e)
        {
            Tool.CallLastPrice();
        }
    }
}
