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
using NLog;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class QuikConnect
    {
        public Quik quik;
        public static Quik Quik = new Quik(Quik.DefaultPort);
        private string clientCode;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //private TradingRobot Robot;

        public QuikConnect()
        {
            //Robot = robot;
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
                DebugLog("Подключаемся к терминалу Quik...", LogType.Info);
                quik = new Quik(port, new InMemoryStorage(), host);

                if (quik != null)
                {
                    DebugLog("Экземпляр Quik создан.", LogType.Info);
                    try
                    {
                        DebugLog("Получаем статус соединения с сервером....", LogType.Info);
                        if (CallQuikConnecting(ref quik))
                        {
                            DebugLog("Соединение с сервером установлено.", LogType.Info);
                            return true;
                        }
                        else
                        {
                            DebugLog("Соединение с сервером НЕ установлено.", LogType.Error);
                            return false;
                        }
                        // для отладки
                        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                        //Trace.Listeners.Add(new TextWriterTraceListener("TraceLogging.log"));
                        // для отладки
                    }
                    catch
                    {
                        DebugLog("Неудачная попытка получить статус соединения с сервером.", LogType.Error);
                        return false;
                    }
                }
            }
            catch
            {
                DebugLog("Ошибка инициализации объекта Quik...", LogType.Error);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Подключение к инструменту.
        /// </summary>
        /// <param name="name_tool"></param>
        /// <returns></returns>
        //public bool ToolConnecting(string name_tool, CandleInterval interval)
        //{
        //    string classCode;
        //    try
        //    {
        //        DebugLog("Определяем код класса инструмента " + name_tool + ", по списку классов" + "...", LogType.Info);
        //        try
        //        {
        //            classCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", name_tool).Result;
        //        }
        //        catch
        //        {
        //            DebugLog("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно", LogType.Info);
        //            return false;
        //        }
        //        if (classCode != null && classCode != "")
        //        {
        //            DebugLog("Определяем код клиента...", LogType.Info);
        //            clientCode = quik.Class.GetClientCode().Result;

        //            DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...", LogType.Info);
        //            Tools.Add(new Tool(ref quik, Tools.Count, name_tool, classCode, interval));

        //            if (Tools.Last() != null && Tools.Last().Name != null && Tools.Last().Name != "" && Tools.Last().SecurityCode == name_tool && Tools.Last().ClassCode == classCode)
        //            {
        //                DebugLog("Инструмент " + Tools.Last().Name + " создан.", LogType.Info);

        //                return true;
        //            }
        //            return false;
        //        }
        //    }
        //    catch
        //    {
        //        DebugLog("Ошибка получения данных по инструменту.", LogType.Info);
        //        return false;
        //    }
        //    return false;
        //}
        public Tool ToolConnectingReturn(string name_tool, CandleInterval interval)
        {
            string classCode;
            try
            {
                DebugLog("Определяем код класса инструмента " + name_tool + ", по списку классов" + "...", LogType.Info);
                try
                {
                    classCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", name_tool).Result;
                }
                catch
                {
                    DebugLog("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно", LogType.Error);
                    return null;
                }
                if (classCode != null && classCode != "")
                {
                    DebugLog("Определяем код клиента...", LogType.Info);
                    clientCode = quik.Class.GetClientCode().Result;

                    DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...", LogType.Info);

                    Tool tool = new Tool(ref quik, 0, name_tool, classCode, interval);///////////

                    if (tool != null && tool.Name != null && tool.Name != "" && tool.SecurityCode == name_tool && tool.ClassCode == classCode)
                    {
                        DebugLog("Инструмент " + tool.Name + " создан.", LogType.Info);

                        return tool;
                    }
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка получения данных по инструменту.", LogType.Error);
                return null;
            }
            return null;
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

        #region Таблицы

        /// <summary>
        /// Возвращает таблицу сделок.
        /// </summary>
        /// <returns></returns>
        public List<Trade> GetTradesTable()
        {
            try
            {
                DebugLog("Получаем таблицу сделок...", LogType.Info);
                List<Trade> trades = quik.Trading.GetTrades().Result;
                DebugLog("Таблица сделок получена.", LogType.Info);
                return trades;
            }
            catch
            {
                DebugLog("Ошибка получения сделок.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Возвращает таблицу заявок.
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOrdersTable()
        {
            try
            {
                DebugLog("Получаем таблицу заявок...", LogType.Info);
                List<Order> orders = quik.Orders.GetOrders().Result;
                DebugLog("Таблица заявок получена.", LogType.Info);
                return orders;
            }
            catch
            {
                DebugLog("Ошибка получения заявок.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Возвращает таблицу стоп-заявок.
        /// </summary>
        /// <returns></returns>
        public List<StopOrder> GetStopOrdersTable()
        {
            try
            {
                DebugLog("Получаем таблицу заявок...", LogType.Info);
                List<StopOrder> stoporders = quik.StopOrders.GetStopOrders().Result;
                DebugLog("Таблица заявок получена.", LogType.Info);
                return stoporders;
            }
            catch
            {
                DebugLog("Ошибка получения заявок.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Получение денежного лимита по фьючерсам.
        /// </summary>
        public decimal GetFuturesDepoClearLimit(string firm_id, string acc_id, int limit_type, string curr_code)
        {
            FuturesLimits futuresLimits = quik.Trading.GetFuturesLimit(firm_id, acc_id, limit_type, curr_code).Result;
            return Convert.ToDecimal(futuresLimits.CbpLPlanned);
        }

        #endregion

        #region Подписки

        /// <summary>
        /// Подписка на стакан.
        /// </summary>
        public bool SubscribeOrderBook(ref Tool Tool)
        {
            try
            {
                DebugLog("Подписываемся на стакан...", LogType.Info);
                quik.OrderBook.Subscribe(Tool.ClassCode, Tool.SecurityCode).Wait();
                Tool.isSubscribedToolOrderBook = quik.OrderBook.IsSubscribed(Tool.ClassCode, Tool.SecurityCode).Result;

                if (Tool.isSubscribedToolOrderBook)
                {
                    DebugLog("Подписка на стакан прошла успешно.", LogType.Info);

                    DebugLog("Подписываемся на изменение стакана (OnQuote)...", LogType.Info);
                    quik.Events.OnQuote += OnQuoteDo;
                    DebugLog("Подписка включена...", LogType.Info);
                    return true;
                }
                else
                {
                    DebugLog("Подписка на стакан не удалась.", LogType.Error);
                    return false;
                }
            }
            catch
            {
                DebugLog("Подписка на стакан не удалась.", LogType.Error);
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
                DebugLog("Подписываемся на изменение позиции на срочном рынке (OnFuturesClientHolding)...", LogType.Info);
                quik.Events.OnFuturesClientHolding += OnFuturesClientHoldingDo;
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции на срочном рынке (OnFuturesClientHolding) не удалась.", LogType.Error);
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
                DebugLog("Подписываемся на получения изменений лимита по бумагам (OnDepoLimit)...", LogType.Info);
                quik.Events.OnDepoLimit += OnDepoLimitDo;
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка на получения изменений лимита по бумагам (OnDepoLimit) не удалась.", LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Подписка инструмента на получение свеч.
        /// </summary>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public bool SubscribeToolReceiveCandles(ref Tool Tool, CandleInterval timeframe)
        {
            try
            {
                while (!Tool.isSubscribedToolCandles)
                {
                    DebugLog($"Подписываем инструмент {Tool.SecurityCode} на получение свеч: " + Tool.ClassCode + " | " + Tool.SecurityCode + " | " + timeframe + "...", LogType.Info);
                    quik.Candles.Subscribe(Tool.ClassCode, Tool.SecurityCode, timeframe).Wait();

                    DebugLog("Проверяем состояние подписки...", LogType.Info);
                    Tool.isSubscribedToolCandles = quik.Candles.IsSubscribed(Tool.ClassCode, Tool.SecurityCode, timeframe).Result;
                }
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка инструмента на получение свеч не удалась.", LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Подписка на получение свеч.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeReceiveCandles()
        {
            try
            {
                DebugLog("Подписываемся на получения свеч (OnNewCandle)...", LogType.Info);
                quik.Candles.NewCandle += OnNewCandle;
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка на получения свеч (OnNewCandle) не удалась.", LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Подписка на получение изменений позиции в стоп-заявках.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeOnStopOrder()
        {
            try
            {
                DebugLog("Подписываемся на изменение позиции в стоп-заявках...", LogType.Info);
                quik.Events.OnStopOrder += OnStopOrderDo;
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции в стоп-заявках не удалась.", LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Подписка на получение информации о новой сделке.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeOnTrade()
        {
            try
            {
                DebugLog("Подписываемся на получение информации о новой сделке...", LogType.Info);
                quik.Events.OnTrade += OnTrade;
                DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                DebugLog("Подписка на получение информации о новой сделке не удалась.", LogType.Error);
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
            DebugLog("Произошло изменение лимита по бумагам", LogType.Info);
        }

        /// <summary>
        /// Обработчик события изменение позиции на срочном рынке
        /// </summary>
        /// <param name="futPos"></param>
        private void OnFuturesClientHoldingDo(FuturesClientHolding futures_position)
        {
            DebugLog("Произошло изменение позиции на срочном рынке", LogType.Info);
        }

        /// <summary>
        /// Обработчик события изменение стакана.
        /// </summary>
        /// <param name="orderbook"></param>
        private void OnQuoteDo(OrderBook orderbook)
        {
            //DebugLog("Произошло изменение стакана", LogType.Info);
        }

        /// <summary>
        /// Обработчик события получения новой свечи.
        /// </summary>
        /// <param name="candle"></param>
        //private void OnNewCandle(Candle candle)
        //{
        //    for (int i = 0; i < mainWindow.Tools.Count; i++)
        //    {
        //        if (mainWindow.Tool.Candles != null && candle.SecCode == mainWindow.Tool.SecurityCode && candle.ClassCode == mainWindow.Tool.ClassCode && candle.Interval == mainWindow.Tool.Interval)
        //        {
        //            mainWindow.Tool.AddNewCandle(new Structures.Candle(candle));

        //            //QuikDateTime temp = mainWindow.Tool.Candles.Last().Datetime;
        //            //DebugLog("Получена новая свеча от: " + temp.day + "." + temp.month + "." + temp.year + " " + temp.hour + "-" + temp.min + "-" + temp.sec + ", значения: " + mainWindow.Tool.Candles.Last().ToString());
        //        }
        //    }
        //}
        private void OnNewCandle(Candle candle)
        {
            //if (Robot.Tool.Candles != null && candle.SecCode == Robot.Tool.SecurityCode && candle.ClassCode == Robot.Tool.ClassCode && candle.Interval == Robot.Tool.Interval)
            //{
            //    Robot.Tool.AddNewCandle(new Structures.Candle(candle));

            //QuikDateTime temp = candle.Datetime;
            //DebugLog("Получена новая свеча из квика от: " + temp.day + "." + temp.month + "." + temp.year + " " + temp.hour + "-" + temp.min + "-" + temp.sec + ", значения: " + candle.ToString());
            //}
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
                    //Trace.WriteLine("Trace: Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State);
                    DebugLog("Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State, LogType.Info);
                    DebugLog("Вызвано событие OnStopOrder - связ. заявка: " + stoporder.LinkedOrder, LogType.Info);
                }
            }
            catch (Exception er)
            {
                //Trace.WriteLine("Trace: Ошибка в OnStopOrderDo() - " + er.ToString());
                DebugLog("Trace: Ошибка в OnStopOrderDo() - " + er.ToString(), LogType.Error);
            }
        }


        private void OnTrade(Trade trade)
        {
            DebugLog("Произошло OnTrade.", LogType.Info);
        }

        #endregion

        #region Отправление заявок

        /// <summary>
        /// Выставление лимитированной заявки.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Tools"></param>
        /// <param name="operation"></param>
        /// <param name="price"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public async Task<Order> LimitOrder(Tool Tool, Operation operation, decimal price, int vol)
        {
            try
            {
                price = Math.Round(price, Tool.PriceAccuracy);
                DebugLog("Выставляем заявку на покупку, по цене:" + price + " ...", LogType.Info);
                long transactionID = (await quik.Orders.SendLimitOrder(Tool.ClassCode, Tool.SecurityCode, Tool.AccountID, operation, price, vol).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    DebugLog("Заявка выставлена. ID транзакции - " + transactionID,LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<Order> Orders = GetOrdersTable();
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tool.ClassCode && order.SecCode == Tool.SecurityCode)
                            {
                                DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum, LogType.Info);
                                return order;
                                //Text2TextBox(textBoxOrderNumber, _order.OrderNum.ToString());
                            }

                        }
                        return null;
                    }
                    catch
                    {
                        DebugLog("Ошибка получения таблицы заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка выставления заявки.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Выставление заявки по рынку.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Tools"></param>
        /// <param name="operation"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public async Task<Order> MarketOrder(Tool Tool, Operation operation, int vol)
        {
            try
            {
                //decimal priceInOrder = Math.Round(Tool.LastPrice + Tool.Step * 5, Tool.PriceAccuracy);
                DebugLog("Выставляем рыночную заявку на покупку...", LogType.Info);
                long transactionID = (await quik.Orders.SendMarketOrder(Tool.ClassCode, Tool.SecurityCode, Tool.AccountID, operation, vol).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    DebugLog("Заявка выставлена. ID транзакции - " + transactionID, LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<Order> Orders = GetOrdersTable();
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tool.ClassCode && order.SecCode == Tool.SecurityCode)
                            {
                                DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum, LogType.Info);
                                //Text2TextBox(textBoxOrderNumber, order.OrderNum.ToString());
                                return order;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        DebugLog("Ошибка получения таблицы заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка выставления заявки.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Выставление стоп-заявки типа тайк-профит и стоп-лимит.
        /// </summary>
        /// <param name="Tool"></param>
        /// <param name="offset"></param>
        /// <param name="spread"></param>
        /// <param name="takeprofit"></param>
        /// <param name="stoplimit"></param>
        /// <param name="price"></param>
        /// <param name="operation"></param>
        /// <param name="vol"></param>
        /// <param name="offset_units"></param>
        /// <param name="spread_unit"></param>
        /// <returns></returns>
        public async Task<StopOrder> TakeProfitStotLimitOrder(Tool Tool, decimal offset, decimal spread,
            decimal takeprofit, decimal stoplimit, decimal price, Operation operation, int vol,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
        {
            try
            {
                StopOrder order = new StopOrder()
                {
                    Account = Tool.AccountID,
                    ClassCode = Tool.ClassCode,
                    ClientCode = clientCode,
                    SecCode = Tool.SecurityCode,
                    Offset = offset,
                    OffsetUnit = offset_units,
                    Spread = spread,
                    SpreadUnit = spread_unit,
                    StopOrderType = StopOrderType.TakeProfitStopLimit,
                    ConditionPrice = Math.Round(takeprofit, Tool.PriceAccuracy),
                    ConditionPrice2 = Math.Round(stoplimit, Tool.PriceAccuracy),
                    Price = Math.Round(price, Tool.PriceAccuracy),
                    Operation = operation,
                    Quantity = vol
                };

                if (operation == Operation.Buy)
                    order.Condition = Condition.MoreOrEqual;
                else
                    order.Condition = Condition.LessOrEqual;

                DebugLog("Выставляем стоп-заявку на покупку, по цене:" + price + "...", LogType.Info);
                long transID = await quik.StopOrders.CreateStopOrder(order).ConfigureAwait(false);
                if (transID > 0)
                {
                    DebugLog("Стоп-заявка выставлена. ID транзакции - " + transID, LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<StopOrder> StopOrders = GetStopOrdersTable();
                        foreach (StopOrder stoporder in StopOrders)
                        {
                            if (stoporder.TransId == transID && stoporder.ClassCode == Tool.ClassCode && stoporder.SecCode == Tool.SecurityCode)
                            {
                                DebugLog("Стоп-заявка выставлена. Номер стоп-заявки - " + stoporder.OrderNum, LogType.Info);
                                return stoporder;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        DebugLog("Ошибка получения таблицы стоп-заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления стоп-заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка выставления стоп-заявки.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Выставление стоп-заявки типа тейк-профит.
        /// </summary>
        /// <param name="Tool"></param>
        /// <param name="offset"></param>
        /// <param name="spread"></param>
        /// <param name="takeprofit"></param>
        /// <param name="price"></param>
        /// <param name="operation"></param>
        /// <param name="vol"></param>
        /// <param name="offset_units"></param>
        /// <param name="spread_unit"></param>
        /// <returns></returns>
        public async Task<StopOrder> TakeProfitOrder(Tool Tool, decimal offset, decimal spread,
            decimal takeprofit, decimal price, Operation operation, int vol,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
        {
            try
            {
                StopOrder order = new StopOrder()
                {
                    Account = Tool.AccountID,
                    ClassCode = Tool.ClassCode,
                    ClientCode = clientCode,
                    SecCode = Tool.SecurityCode,
                    Offset = offset,
                    OffsetUnit = offset_units,
                    Spread = spread,
                    SpreadUnit = spread_unit,
                    StopOrderType = StopOrderType.TakeProfit,
                    ConditionPrice = Math.Round(takeprofit, Tool.PriceAccuracy),
                    Price = Math.Round(price, Tool.PriceAccuracy),
                    Operation = operation,
                    Quantity = vol
                };

                if (operation == Operation.Sell)
                    order.Condition = Condition.MoreOrEqual;
                else
                    order.Condition = Condition.LessOrEqual;

                DebugLog("Выставляем тейк-профит, по цене:" + price + "...", LogType.Info);
                long transID = await quik.StopOrders.CreateStopOrder(order).ConfigureAwait(false);
                if (transID > 0)
                {
                    DebugLog("Стоп-заявка выставлена. ID транзакции - " + transID, LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<StopOrder> StopOrders = GetStopOrdersTable();
                        foreach (StopOrder stoporder in StopOrders)
                        {
                            if (stoporder.TransId == transID && stoporder.ClassCode == Tool.ClassCode && stoporder.SecCode == Tool.SecurityCode)
                            {
                                DebugLog("Стоп-заявка выставлена. Номер стоп-заявки - " + stoporder.OrderNum, LogType.Info);
                                return stoporder;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        DebugLog("Ошибка получения таблицы стоп-заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления стоп-заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка выставления стоп-заявки.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Выставление стоп-заявки типа стоп-лимит. 
        /// </summary>
        /// <param name="Tool"></param>
        /// <param name="offset"></param>
        /// <param name="spread"></param>
        /// <param name="stoploss"></param>
        /// <param name="price"></param>
        /// <param name="operation"></param>
        /// <param name="vol"></param>
        /// <param name="offset_units"></param>
        /// <param name="spread_unit"></param>
        /// <returns></returns>
        public async Task<StopOrder> StopLimitOrder(Tool Tool, decimal offset, decimal spread,
            decimal stoploss, decimal price, Operation operation, int vol,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
        {
            try
            {
                StopOrder order = new StopOrder()
                {
                    Account = Tool.AccountID,
                    ClassCode = Tool.ClassCode,
                    ClientCode = clientCode,
                    SecCode = Tool.SecurityCode,
                    Offset = offset,
                    OffsetUnit = offset_units,
                    Spread = spread,
                    SpreadUnit = spread_unit,
                    StopOrderType = StopOrderType.StopLimit,
                    ConditionPrice = Math.Round(stoploss, Tool.PriceAccuracy),
                    Price = Math.Round(price, Tool.PriceAccuracy),
                    Operation = operation,
                    Quantity = vol
                };

                if (operation == Operation.Sell)
                    order.Condition = Condition.LessOrEqual;
                else
                    order.Condition = Condition.MoreOrEqual;

                DebugLog("Выставляем стоп-лимит, по цене: " + price + "...", LogType.Info);
                long transID = await quik.StopOrders.CreateStopOrder(order).ConfigureAwait(false);
                if (transID > 0)
                {
                    DebugLog("Стоп-заявка выставлена. ID транзакции - " + transID, LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<StopOrder> StopOrders = GetStopOrdersTable();
                        foreach (StopOrder stoporder in StopOrders)
                        {
                            if (stoporder.TransId == transID && stoporder.ClassCode == Tool.ClassCode && stoporder.SecCode == Tool.SecurityCode)
                            {
                                DebugLog("Стоп-заявка выставлена. Номер стоп-заявки - " + stoporder.OrderNum, LogType.Info);
                                return stoporder;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        DebugLog("Ошибка получения таблицы стоп-заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    DebugLog("Неудачная попытка выставления стоп-заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                DebugLog("Ошибка выставления стоп-заявки.", LogType.Error);
                return null;
            }
        }

        /// <summary>
        /// Снятие заявки.
        /// </summary>
        /// <param name="id_order"></param>
        /// <returns></returns>
        public async Task TakeOffOrder(long id_order)
        {
            try
            {
                List<Order> Orders = GetOrdersTable();
                int index_order = Orders.FindIndex(o => o.OrderNum == id_order);
                if (Orders[index_order] != null && Orders[index_order].OrderNum > 0)
                    DebugLog("Снимаем заявку на покупку с номером - " + Orders[index_order].OrderNum + "...", LogType.Info);
                await quik.Orders.KillOrder(Orders[index_order]);
                DebugLog("Заявка снята.", LogType.Info);
            }
            catch
            {
                DebugLog("Ошибка снятия заявки.", LogType.Error);
            }
        }

        /// <summary>
        /// Снятие стоп-заявки.
        /// </summary>
        /// <param name="id_order"></param>
        /// <returns></returns>
        public async Task TakeOffStopOrder(long id_order)
        {
            try
            {
                List<StopOrder> StopOrders = GetStopOrdersTable();
                int index_order = StopOrders.FindIndex(o => o.OrderNum == id_order);
                if (StopOrders[index_order] != null && StopOrders[index_order].OrderNum > 0) DebugLog("Снимаем стоп-заявку с номером - " + StopOrders[index_order].OrderNum + "...", LogType.Info);
                await quik.StopOrders.KillStopOrder(StopOrders[index_order]);
                DebugLog("Заявка снята.", LogType.Info);
            }
            catch
            {
                DebugLog("Ошибка снятия заявки.", LogType.Error);
            }
        }

        #endregion

        #region Лог

        public void DebugLog(string log_string, LogType log_type)
        {
            Debug.WriteLine(log_string);
            //mainWindow.Log += log_string + "\r\n";

            switch (log_type)
            {
                case LogType.Null:
                    break;
                case LogType.Debug:
                    logger.Debug(log_string);
                    break;
                case LogType.Info:
                    logger.Info(log_string);
                    break;
                case LogType.Warn:
                    logger.Warn(log_string);
                    break;
                case LogType.Error:
                    logger.Error(log_string);
                    break;
                case LogType.Fatal:
                    logger.Fatal(log_string);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
