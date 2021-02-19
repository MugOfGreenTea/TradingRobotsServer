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
        public Thread ThreadConnectQuik;
        public Timer TimerConnectQuik;
        public Thread ThreadWaitingForConnection;
        public Timer TimerWaitingForConnection;
        public Thread ThreadWaitingForTool;
        public Timer TimerWaitingForTool;
        public Strategy Strategy;

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

        public void Run(int port, string host, string tool_name, CandleInterval candle_interval, string param)
        {
            ToolName = tool_name;
            Interval = candle_interval;
            Param = param;
            InitialQuik();
            ConnectQuik(port, host);
            if (check_quik_connecting)
            {
                ConnectTool(tool_name, candle_interval);
                RunStrategy(param);
            }
            else
            {
                WaitingForConnecting();
            }
        }

        private void InitialQuik()
        {
            quik_connect = new QuikConnect();
        }

        private void ConnectQuik(int port, string host)
        {
            check_quik_connecting = quik_connect.QuikConnecting(port, host);

            ThreadConnectQuik = new Thread(new ThreadStart(StartTimerQuikConnect));
            ThreadConnectQuik.Start();
        }

        private void WaitingForConnecting()
        {
            ThreadWaitingForConnection = new Thread(new ThreadStart(StartTimerWaitingForConnection));
            ThreadWaitingForConnection.Start();
            check_waiting_for_connection = true;
        }

        private void StartTimerQuikConnect()
        {
            TimerConnectQuik = new Timer();
            TimerConnectQuik.Interval = 5000;
            TimerConnectQuik.Elapsed += CallQuikConnect;
            TimerConnectQuik.Start();
        }

        private void CallQuikConnect(object sender, EventArgs e)
        {
            check_quik_connecting = quik_connect.CallQuikConnecting();
            if (!check_quik_connecting && !check_waiting_for_connection)
                WaitingForConnecting();
            //Debug.WriteLine("Соединение с сервером установленно.");
        }

        private void StartTimerWaitingForConnection()
        {
            TimerConnectQuik = new Timer();
            TimerConnectQuik.Interval = 60000;
            TimerConnectQuik.Elapsed += CallWaitingForConnection;
            TimerConnectQuik.Start();
        }

        private void CallWaitingForConnection(object sender, EventArgs e)
        {
            if (check_quik_connecting)
            {
                ConnectTool(ToolName, Interval);
                RunStrategy(Param);
                TimerWaitingForConnection.Stop();
                ThreadWaitingForConnection.Abort();
                TimerWaitingForConnection = null;
                ThreadWaitingForConnection = null;
                check_waiting_for_connection = false;
            }
            Debug.WriteLine("Ожидание подключения к серверу.");
        }

        private void ConnectTool(string tool_name, CandleInterval candle_interval)
        {
            if (check_quik_connecting)
            {
                Tool = quik_connect.ToolConnectingReturn(tool_name, candle_interval);
            }

            if (Tool != null)
            {
                check_subscribe_tool_candles = quik_connect.SubscribeToolReceiveCandles(ref Tool, candle_interval);

                Tool.SubscribeNewCandle();//подписка Tool на новые свечи от QuikConnect
            }
        }

        private void RunStrategy(string param)
        {
            if (check_subscribe_tool_candles && check_subscribe_orderbook)
            {
                ThreadToolStrategy = new Thread(new ThreadStart(StartTimerCallLastPrice));
                ThreadToolStrategy.Start();
            }

            Strategy = new SetPositionByCandleHighLowStrategy(param, Bot);
            Bot = new Bot(quik_connect, Tool, Strategy);

            Strategy.Bot = Bot;
            Strategy.SubsribeNewDeal();
            Bot.SubsribeNewCandleToolAsync();//подписка Bot на новые свечи от Tool
            Bot.SubsribeNewOrderStrategy();//подписка Bot на новые заявки от Strategy
            Bot.SubsribeOnOrder();//подписка Bot на новые ордера
            Bot.SubscribeOnStopOrder();//подписка Bot на новые стопордера

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
