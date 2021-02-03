using QuikSharp;
using QuikSharp.DataStructures;
using Candle = QuikSharp.DataStructures.Candle;
using QuikSharp.DataStructures.Transaction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;
using TradingRobotsServer.ViewModels;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class QuikConnect
    {
        public Quik quik;
        public Tools Tools;
        private string clientCode;

        private RobotPanelViewModel mainWindow;

        public QuikConnect(RobotPanelViewModel mainWindowViewModel)
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
                    Tools.Add(new Tool(ref quik, Tools.Count, name_tool, classCode, interval));

                    if (Tools.Last() != null && Tools.Last().Name != null && Tools.Last().Name != "" && Tools.Last().SecurityCode == name_tool && Tools.Last().ClassCode == classCode)
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
                    DebugLog("Подписка включена...");
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
                DebugLog("Подписка включена...");
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
                DebugLog("Подписка включена...");
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
                DebugLog("Подписка на получение свеч не удалась.");
                return false;
            }
        }

        public bool SubscribeOnStopOrder()
        {
            try
            {
                DebugLog("Подписываемся на изменение позиции в стоп-заявках...");
                quik.Events.OnStopOrder += OnStopOrderDo;
                DebugLog("Подписка включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции в стоп-заявках не удалась.");
                return false;
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик события изменение лимита по бумагам.
        /// </summary>
        /// <param name="dLimit"></param>
        private void OnDepoLimitDo(DepoLimitEx depo_limit)
        {
            DebugLog("Произошло изменение лимита по бумагам");
        }

        /// <summary>
        /// Обработчик события изменение позиции на срочном рынке
        /// </summary>
        /// <param name="futPos"></param>
        private void OnFuturesClientHoldingDo(FuturesClientHolding futures_position)
        {
            DebugLog("Произошло изменение позиции на срочном рынке");
        }

        /// <summary>
        /// Обработчик события изменение стакана.
        /// </summary>
        /// <param name="orderbook"></param>
        private void OnQuoteDo(OrderBook orderbook)
        {
            //DebugLog("Произошло изменение стакана");
        }

        /// <summary>
        /// Обработчик события получения новой свечи.
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

        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private void OnStopOrderDo(StopOrder stoporder)
        {
            try
            {
                if (stoporder != null && stoporder.OrderNum > 0)
                {
                    Trace.WriteLine("Trace: Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State);
                    DebugLog("Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State);
                }
            }
            catch (Exception er)
            {
                Trace.WriteLine("Trace: Ошибка в OnStopOrderDo() - " + er.ToString());
                DebugLog("Trace: Ошибка в OnStopOrderDo() - " + er.ToString());
            }
        }

        #endregion

        #region Отправление заявок

        public async Task LimitOrder(int i, Operation operation, decimal price, int vol)
        {
            try
            {
                decimal priceInOrder = Math.Round(Tools[i].LastPrice + Tools[i].Step * 5, Tools[i].PriceAccuracy);
                DebugLog("Выставляем заявку на покупку, по цене:" + priceInOrder + " ...");
                long transactionID = (await quik.Orders.SendLimitOrder(Tools[i].ClassCode, Tools[i].SecurityCode, Tools[i].AccountID, operation, price, vol).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    DebugLog("Заявка выставлена. ID транзакции - " + transactionID);
                    Thread.Sleep(500);
                    try
                    {
                        List<Order> Orders = quik.Orders.GetOrders().Result;
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tools[i].ClassCode && order.SecCode == Tools[i].SecurityCode)
                            {
                                DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum);
                                //Text2TextBox(textBoxOrderNumber, _order.OrderNum.ToString());
                            }
                            else
                            {
                                //Text2TextBox(textBoxOrderNumber, "---");
                            }
                        }
                    }
                    catch
                    {
                        DebugLog("Ошибка получения номера заявки.");
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления заявки.");
                }
            }
            catch
            {
                DebugLog("Ошибка выставления заявки.");
            }
        }

        public async Task MarketOrder(int i, Operation operation, int vol)
        {
            try
            {
                //decimal priceInOrder = Math.Round(Tools[i].LastPrice + Tools[i].Step * 5, Tools[i].PriceAccuracy);
                DebugLog("Выставляем рыночную заявку на покупку...");
                long transactionID = (await quik.Orders.SendMarketOrder(Tools[i].ClassCode, Tools[i].SecurityCode, Tools[i].AccountID, Operation.Buy, 1).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    DebugLog("Заявка выставлена. ID транзакции - " + transactionID);
                    Thread.Sleep(500);
                    try
                    {
                        List<Order> Orders = quik.Orders.GetOrders().Result;
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tools[i].ClassCode && order.SecCode == Tools[i].SecurityCode)
                            {
                                DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum);
                                //Text2TextBox(textBoxOrderNumber, order.OrderNum.ToString());
                            }
                            else
                            {
                                //Text2TextBox(textBoxOrderNumber, "---");
                            }
                        }
                    }
                    catch
                    {
                        DebugLog("Ошибка получения номера заявки.");
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления заявки.");
                }
            }
            catch
            {
                DebugLog("Ошибка выставления заявки.");
            }
        }

        public async Task TakeProfitStotLimitOrder(int i, decimal offset, decimal spread, Condition condition, 
            decimal takeprofit, decimal stoplimit, decimal price, Operation operation, int vol, 
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
        {
            try
            {
                StopOrder order = new StopOrder()
                {
                    Account = Tools[i].AccountID,
                    ClassCode = Tools[i].ClassCode,
                    ClientCode = clientCode,
                    SecCode = Tools[i].SecurityCode,
                    Offset = offset,
                    OffsetUnit = offset_units,
                    Spread = spread,
                    SpreadUnit = spread_unit,
                    StopOrderType = StopOrderType.TakeProfitStopLimit,
                    Condition = condition,
                    ConditionPrice = takeprofit,
                    ConditionPrice2 = stoplimit,
                    Price = price,
                    Operation = operation,
                    Quantity = vol
                };

                DebugLog("Выставляем стоп-заявку на покупку, по цене:" + price + "...");
                long transID = await quik.StopOrders.CreateStopOrder(order).ConfigureAwait(false);
                if (transID > 0)
                {
                    DebugLog("Заявка выставлена. ID транзакции - " + transID);
                    Thread.Sleep(500);
                    try
                    {
                        List<StopOrder> StopOrders = quik.StopOrders.GetStopOrders().Result;
                        foreach (StopOrder stoporder in StopOrders)
                        {
                            if (stoporder.TransId == transID && stoporder.ClassCode == Tools[i].ClassCode && stoporder.SecCode == Tools[i].SecurityCode)
                            {
                                DebugLog("Стоп-заявка выставлена. Номер стоп-заявки - " + stoporder.OrderNum);
                            }
                        }
                    }
                    catch
                    {
                        DebugLog("Ошибка получения номера стоп-заявки.");
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления стоп-заявки.");
                }
            }
            catch
            {
                DebugLog("Ошибка выставления стоп-заявки.");
            }
        }

        public async Task TakeOffOrder(long id_order)
        {
            try
            {
                List<Order> Orders = quik.Orders.GetOrders().Result;
                int index_order = Orders.FindIndex(o => o.OrderNum == id_order);
                if (Orders[index_order] != null && Orders[index_order].OrderNum > 0) 
                    DebugLog("Удаляем заявку на покупку с номером - " + Orders[index_order].OrderNum + " ...");
                long x = quik.Orders.KillOrder(Orders[index_order]).Result;
                DebugLog("Результат - " + x + " ...");
            }
            catch
            {
                DebugLog("Ошибка удаления заявки.");
            }
        }

        public async Task TakeOffStopOrder(long id_order)
        {
            try
            {
                List<StopOrder> StopOrders = quik.StopOrders.GetStopOrders().Result;
                int index_order = StopOrders.FindIndex(o => o.OrderNum == id_order);
                if (StopOrders[index_order] != null && StopOrders[index_order].OrderNum > 0) DebugLog("Удаляем заявку на покупку с номером - " + StopOrders[index_order].OrderNum + " ...");
                long x = quik.StopOrders.KillStopOrder(StopOrders[index_order]).Result;
                DebugLog("Результат - " + x + " ...");
            }
            catch
            {
                DebugLog("Ошибка удаления заявки.");
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
