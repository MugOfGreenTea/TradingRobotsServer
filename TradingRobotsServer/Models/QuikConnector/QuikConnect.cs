using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Diagnostics;
using System.Linq;
using TradingRobotsServer.Models.Structures;
using TradingRobotsServer.ViewModels;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class QuikConnect
    {
        public Quik quik;
        public Tools Tools;
        private string clientCode;
        private MainWindowViewModel mainWindow;

        public QuikConnect(MainWindowViewModel mainWindowViewModel)
        {
            mainWindow = mainWindowViewModel;
            Tools = new Tools();
        }

        #region Подключения

        /// <summary>
        /// Подключение к Quik.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool QuikConnecting(int port, string host)
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
                            return true;
                        }
                        else
                        {
                            DebugLog("Соединение с сервером НЕ установлено.");
                            return false;
                        }
                        // для отладки
                        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                        //Trace.Listeners.Add(new TextWriterTraceListener("TraceLogging.log"));
                        // для отладки
                    }
                    catch
                    {
                        DebugLog("Неудачная попытка получить статус соединения с сервером.");
                        return false;
                    }
                }
            }
            catch
            {
                DebugLog("Ошибка инициализации объекта Quik...");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Подключение к инструменту.
        /// </summary>
        /// <param name="name_tool"></param>
        /// <returns></returns>
        public bool ToolConnecting(string name_tool, CandleInterval interval)
        {
            string classCode;
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
                    return false;
                }
                if (classCode != null && classCode != "")
                {
                    DebugLog("Определяем код клиента...");
                    clientCode = quik.Class.GetClientCode().Result;

                    DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...");
                    Tools.Add(new Tool(ref quik, name_tool, classCode, interval));

                    if (Tools.Last() != null && Tools.Last().Name != null && Tools.Last().Name != "" && Tools.Last().Name == name_tool && Tools.Last().ClassCode == classCode)
                    {
                        DebugLog("Инструмент " + Tools.Last().Name + " создан.");

                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                DebugLog("Ошибка получения данных по инструменту.");
                return false;
            }
            return false;
        }

        #endregion

        #region Проверки статуса

        /// <summary>
        /// Проверка подключения к Quik.
        /// </summary>
        /// <param name="quik"></param>
        /// <returns></returns>
        public static bool CallQuikConnecting(ref Quik quik)
        {
            return quik.Service.IsConnected().Result;
        }

        public static bool CallSubscribeCandle(ref Quik quik, ref Tool tool, CandleInterval timeframe)
        {
            return quik.Candles.IsSubscribed(tool.ClassCode, tool.SecurityCode, timeframe).Result;
        }

        #endregion

        #region Подписки

        /// <summary>
        /// Подписка на стакан.
        /// </summary>
        public bool SubscribeOrderBook(int index_tool)
        {
            try
            {
                DebugLog("Подписываемся на стакан...");
                quik.OrderBook.Subscribe(Tools[index_tool].ClassCode, Tools[index_tool].SecurityCode).Wait();
                Tools[index_tool].isSubscribedToolOrderBook = quik.OrderBook.IsSubscribed(Tools[index_tool].ClassCode, Tools[index_tool].SecurityCode).Result;

                if (Tools[index_tool].isSubscribedToolOrderBook)
                {
                    DebugLog("Подписка на стакан прошла успешно.");

                    DebugLog("Подписываемся на изменение стакана (OnQuote)...");
                    quik.Events.OnQuote += OnQuoteDo;
                    return true;
                }
                else
                {
                    DebugLog("Подписка на стакан не удалась.");
                    return false;
                }
            }
            catch
            {
                DebugLog("Подписка на стакан не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Подписка на событие изменения позиции на срочном рынке.
        /// </summary>
        public bool SubscribeOnFuturesClientHolding()
        {
            try
            {
                DebugLog("Подписываемся на изменение позиции на срочном рынке (OnFuturesClientHolding)...");
                quik.Events.OnFuturesClientHolding += OnFuturesClientHoldingDo;
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции на срочном рынке (OnFuturesClientHolding) не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Подписка на событие получения изменений лимита по бумагам.
        /// </summary>
        public bool SubscribeOnDepoLimit()
        {
            try
            {
                DebugLog("Подписываемся на получения изменений лимита по бумагам (OnDepoLimit)...");
                quik.Events.OnDepoLimit += OnDepoLimitDo;
                return true;
            }
            catch
            {
                DebugLog("Подписка на получения изменений лимита по бумагам (OnDepoLimit) не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Подписка на получение свеч.
        /// </summary>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public bool SubscribeReceiveCandles(int index_tool, CandleInterval timeframe)
        {
            try
            {
                while (!Tools[index_tool].isSubscribedToolCandles)
                {
                    DebugLog("Подписываемся на получение свечек: " + Tools[index_tool].ClassCode + " | " + Tools[index_tool].SecurityCode + " | " + timeframe + "...");
                    quik.Candles.Subscribe(Tools[index_tool].ClassCode, Tools[index_tool].SecurityCode, timeframe).Wait();

                    DebugLog("Проверяем состояние подписки...");
                    Tools[index_tool].isSubscribedToolCandles = quik.Candles.IsSubscribed(Tools[index_tool].ClassCode, Tools[index_tool].SecurityCode, timeframe).Result;

                    quik.Candles.NewCandle += OnNewCandle;
                }
                DebugLog("Подписка включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на получение свеч не удалась");
                return false;
            }
        }

        #endregion

        #region Обработчики событий

        private void OnDepoLimitDo(DepoLimitEx dLimit)
        {
            DebugLog("Произошло изменение лимита по бумагам");
        }

        private void OnFuturesClientHoldingDo(FuturesClientHolding futPos)
        {
            DebugLog("Произошло изменение позиции на срочном рынке");
        }

        private void OnQuoteDo(OrderBook orderbook)
        {
            //DebugLog("Произошло изменение стакана");
        }

        /// <summary>
        /// Обрабодчик события получения новой свечи.
        /// </summary>
        /// <param name="candle"></param>
        private void OnNewCandle(Candle candle)
        {
            for (int i = 0; i < Tools.Count; i++)
            {
                if (Tools[i].Candles != null && candle.SecCode == Tools[i].SecurityCode && candle.ClassCode == Tools[i].ClassCode && candle.Interval == Tools[i].Interval)
                {
                    Tools[i].AddNewCandle(candle);

                    QuikDateTime temp = Tools[i].Candles.Last().Datetime;
                    DebugLog("Получена новая свеча от: " + temp.day + "." + temp.month + "." + temp.year + " " + temp.hour + "-" + temp.min + "-" + temp.sec + ", значения: " + Tools[i].Candles.Last().ToString());
                }
            }
        }

        #endregion

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
            mainWindow.Log += log_string + "\r\n";
        }

        #endregion
    }
}
