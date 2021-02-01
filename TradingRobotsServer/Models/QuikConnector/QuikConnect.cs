using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class QuikConnect
    {
        public Quik quik;
        public Tool tool;
        private bool isSubscribedToolOrderBook = false;
        private bool isSubscribedToolCandles = false;
        private string secCode = "";
        private string classCode = "";
        private string clientCode;
        private decimal bid;
        private decimal offer;
        private OrderBook toolOrderBook;


        string Log;

        public QuikConnect(ref string log)
        {

        }

        /// <summary>
        /// Подключение к Quik.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public void QuikConnecting(int port, string host)
        {
            try
            {
                DebugLog("Подключаемся к терминалу Quik...");
                quik = new Quik(port, new InMemoryStorage(), host);

                if (quik != null)
                {
                    DebugLog("Экземпляр Quik создан.");
                    try
                    {
                        DebugLog("Получаем статус соединения с сервером....");
                        if (CallQuikConnecting(ref quik))
                        {
                            DebugLog("Соединение с сервером установлено.");
                        }
                        else
                        {
                            DebugLog("Соединение с сервером НЕ установлено.");
                        }
                        // для отладки
                        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                        //Trace.Listeners.Add(new TextWriterTraceListener("TraceLogging.log"));
                        // для отладки
                    }
                    catch
                    {
                        DebugLog("Неудачная попытка получить статус соединения с сервером.");
                    }
                }
            }
            catch
            {
                DebugLog("Ошибка инициализации объекта Quik...");
            }
        }

        public static bool CallQuikConnecting(ref Quik quik)
        {
            return quik.Service.IsConnected().Result;
        }

        public void ToolConnecting(string name_tool)
        {
            try
            {
                DebugLog("Определяем код класса инструмента " + name_tool + ", по списку классов" + "...");
                try
                {
                    classCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", name_tool).Result;
                }
                catch
                {
                    DebugLog("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно");
                }
                if (classCode != null && classCode != "")
                {
                    DebugLog("Определяем код клиента...");
                    clientCode = quik.Class.GetClientCode().Result;
                    DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...");
                    tool = new Tool(ref quik, name_tool, classCode);
                    if (tool != null && tool.Name != null && tool.Name != "")
                    {
                        DebugLog("Инструмент " + tool.Name + " создан.");
                        DebugLog("Подписываемся на стакан...");
                        quik.OrderBook.Subscribe(tool.ClassCode, tool.SecurityCode).Wait();
                        isSubscribedToolOrderBook = quik.OrderBook.IsSubscribed(tool.ClassCode, tool.SecurityCode).Result;
                        if (isSubscribedToolOrderBook)
                        {
                            toolOrderBook = new OrderBook();
                            DebugLog("Подписка на стакан прошла успешно.");

                            DebugLog("Подписываемся на колбэк 'OnQuote'...");
                            quik.Events.OnQuote += OnQuoteDo;
                        }
                        else
                        {
                            DebugLog("Подписка на стакан не удалась.");
                        }
                        DebugLog("Подписываемся на колбэк 'OnFuturesClientHolding'...");
                        quik.Events.OnFuturesClientHolding += OnFuturesClientHoldingDo;
                        DebugLog("Подписываемся на колбэк 'OnDepoLimit'...");
                        quik.Events.OnDepoLimit += OnDepoLimitDo;
                    }

                }
            }
            catch
            {
                DebugLog("Ошибка получения данных по инструменту.");
            }

        }

        private void OnDepoLimitDo(DepoLimitEx dLimit)
        {
            throw new NotImplementedException();
        }

        private void OnFuturesClientHoldingDo(FuturesClientHolding futPos)
        {
            throw new NotImplementedException();
        }

        private void OnQuoteDo(OrderBook orderbook)
        {
            throw new NotImplementedException();
        }

        private void OnNewCandle(Candle candle)
        {
            if (toolCandles != null) NewCandle_to_Table(ref toolCandles, candle);
        }

        public static void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }
    }
}
