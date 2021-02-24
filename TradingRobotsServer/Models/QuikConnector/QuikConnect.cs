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
using System.Globalization;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class QuikConnect
    {
        public Quik quik;
        public static Quik Quik = new Quik(Quik.DefaultPort);
        private string clientCode;

        //private TradingRobot Robot;
        private Char separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

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
                Logs.DebugLog("Подключаемся к терминалу Quik...", LogType.Info);
                quik = new Quik(port, new InMemoryStorage(), host);

                if (quik != null)
                {
                    Logs.DebugLog("Экземпляр Quik создан.", LogType.Info);
                    try
                    {
                        Logs.DebugLog("Получаем статус соединения с сервером....", LogType.Info);
                        if (CallQuikConnecting())
                        {
                            Logs.DebugLog("Соединение с сервером установлено.", LogType.Info);
                            return true;
                        }
                        else
                        {
                            Logs.DebugLog("Соединение с сервером НЕ установлено.", LogType.Error);
                            return false;
                        }
                        // для отладки
                        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                        //Trace.Listeners.Add(new TextWriterTraceListener("TraceLogging.log"));
                        // для отладки
                    }
                    catch
                    {
                        Logs.DebugLog("Неудачная попытка получить статус соединения с сервером.", LogType.Error);
                        return false;
                    }
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка инициализации объекта Quik...", LogType.Error);
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
        //        Logs.DebugLog("Определяем код класса инструмента " + name_tool + ", по списку классов" + "...", LogType.Info);
        //        try
        //        {
        //            classCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", name_tool).Result;
        //        }
        //        catch
        //        {
        //            Logs.DebugLog("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно", LogType.Info);
        //            return false;
        //        }
        //        if (classCode != null && classCode != "")
        //        {
        //            Logs.DebugLog("Определяем код клиента...", LogType.Info);
        //            clientCode = quik.Class.GetClientCode().Result;

        //            Logs.DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...", LogType.Info);
        //            Tools.Add(new Tool(ref quik, Tools.Count, name_tool, classCode, interval));

        //            if (Tools.Last() != null && Tools.Last().Name != null && Tools.Last().Name != "" && Tools.Last().SecurityCode == name_tool && Tools.Last().ClassCode == classCode)
        //            {
        //                Logs.DebugLog("Инструмент " + Tools.Last().Name + " создан.", LogType.Info);

        //                return true;
        //            }
        //            return false;
        //        }
        //    }
        //    catch
        //    {
        //        Logs.DebugLog("Ошибка получения данных по инструменту.", LogType.Info);
        //        return false;
        //    }
        //    return false;
        //}
        public Tool ToolConnectingReturn(string name_tool, CandleInterval interval)
        {
            string classCode;
            try
            {
                Logs.DebugLog("Определяем код класса инструмента " + name_tool + ", по списку классов" + "...", LogType.Info);
                try
                {
                    classCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", name_tool).Result;
                }
                catch
                {
                    Logs.DebugLog("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно", LogType.Error);
                    return null;
                }
                if (classCode != null && classCode != "")
                {
                    Logs.DebugLog("Определяем код клиента...", LogType.Info);
                    clientCode = quik.Class.GetClientCode().Result;

                    Logs.DebugLog("Создаем экземпляр инструмента " + name_tool + "|" + classCode + "...", LogType.Info);

                    Tool tool = new Tool(ref quik, 0, name_tool, classCode, interval);///////////

                    if (tool != null && tool.Name != null && tool.Name != "" && tool.SecurityCode == name_tool && tool.ClassCode == classCode)
                    {
                        Logs.DebugLog("Инструмент " + tool.Name + " создан.", LogType.Info);

                        return tool;
                    }
                    return null;
                }
                else
                {
                    Logs.DebugLog("Ошибка получения данных по инструменту.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка получения данных по инструменту.", LogType.Error);
                return null;
            }
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
        public bool CallQuikConnecting()
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
                Logs.DebugLog("Получаем таблицу сделок...", LogType.Info);
                List<Trade> trades = quik.Trading.GetTrades().Result;
                Logs.DebugLog("Таблица сделок получена.", LogType.Info);
                return trades;
            }
            catch
            {
                Logs.DebugLog("Ошибка получения сделок.", LogType.Error);
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
                Logs.DebugLog("Получаем таблицу заявок...", LogType.Info);
                List<Order> orders = quik.Orders.GetOrders().Result;
                Logs.DebugLog("Таблица заявок получена.", LogType.Info);
                return orders;
            }
            catch
            {
                Logs.DebugLog("Ошибка получения заявок.", LogType.Error);
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
                Logs.DebugLog("Получаем таблицу заявок...", LogType.Info);
                List<StopOrder> stoporders = quik.StopOrders.GetStopOrders().Result;
                Logs.DebugLog("Таблица заявок получена.", LogType.Info);
                return stoporders;
            }
            catch
            {
                Logs.DebugLog("Ошибка получения заявок.", LogType.Error);
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

        /// <summary>
        /// Получение информации из таблицы текущих торгов.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="param_names"></param>
        /// <returns></returns>
        public string GetParamEx(Param param, ParamNames param_names)
        {
            return quik.Trading.GetParamEx(param.ClassCode, param.SecCode, param_names).Result.ParamValue.Replace('.', separator);
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
                Logs.DebugLog("Подписываемся на стакан...", LogType.Info);
                quik.OrderBook.Subscribe(Tool.ClassCode, Tool.SecurityCode).Wait();
                Tool.isSubscribedToolOrderBook = quik.OrderBook.IsSubscribed(Tool.ClassCode, Tool.SecurityCode).Result;

                if (Tool.isSubscribedToolOrderBook)
                {
                    Logs.DebugLog("Подписка на стакан прошла успешно.", LogType.Info);

                    Logs.DebugLog("Подписываемся на изменение стакана (OnQuote)...", LogType.Info);
                    quik.Events.OnQuote += OnQuoteDo;
                    Logs.DebugLog("Подписка включена...", LogType.Info);
                    return true;
                }
                else
                {
                    Logs.DebugLog("Подписка на стакан не удалась.", LogType.Error);
                    return false;
                }
            }
            catch
            {
                Logs.DebugLog("Подписка на стакан не удалась.", LogType.Error);
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
                Logs.DebugLog("Подписываемся на изменение позиции на срочном рынке (OnFuturesClientHolding)...", LogType.Info);
                quik.Events.OnFuturesClientHolding += OnFuturesClientHoldingDo;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на изменение позиции на срочном рынке (OnFuturesClientHolding) не удалась.", LogType.Error);
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
                Logs.DebugLog("Подписываемся на получения изменений лимита по бумагам (OnDepoLimit)...", LogType.Info);
                quik.Events.OnDepoLimit += OnDepoLimitDo;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получения изменений лимита по бумагам (OnDepoLimit) не удалась.", LogType.Error);
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
                    Logs.DebugLog($"Подписываем инструмент {Tool.SecurityCode} на получение свеч: " + Tool.ClassCode + " | " + Tool.SecurityCode + " | " + timeframe + "...", LogType.Info);
                    quik.Candles.Subscribe(Tool.ClassCode, Tool.SecurityCode, timeframe).Wait();

                    Logs.DebugLog("Проверяем состояние подписки...", LogType.Info);
                    Tool.isSubscribedToolCandles = quik.Candles.IsSubscribed(Tool.ClassCode, Tool.SecurityCode, timeframe).Result;
                }
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка инструмента на получение свеч не удалась.", LogType.Error);
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
                Logs.DebugLog("Подписываемся на получения свеч (OnNewCandle)...", LogType.Info);
                quik.Candles.NewCandle += OnNewCandle;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получения свеч (OnNewCandle) не удалась.", LogType.Error);
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
                Logs.DebugLog("Подписываемся на изменение позиции в стоп-заявках...", LogType.Info);
                quik.Events.OnStopOrder += OnStopOrderDo;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на изменение позиции в стоп-заявках не удалась.", LogType.Error);
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
                Logs.DebugLog("Подписываемся на получение информации о новой сделке...", LogType.Info);
                quik.Events.OnTrade += OnTrade;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получение информации о новой сделке не удалась.", LogType.Error);
                return false;
            }
        }

        public bool SubscribeOnTransReply()
        {
            try
            {
                Logs.DebugLog("Подписываемся на получение информации о новой сделке...", LogType.Info);
                quik.Events.OnTransReply += OnTransReply;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получение информации о новой сделке не удалась.", LogType.Error);
                return false;
            }
        }

        public bool SubscribeOnParam()
        {
            try
            {
                Logs.DebugLog("Подписываемся на получение информации о новой сделке...", LogType.Info);
                quik.Events.OnParam += OnParam;
                Logs.DebugLog("Подписка включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получение информации о новой сделке не удалась.", LogType.Error);
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
            Logs.DebugLog("Произошло изменение лимита по бумагам", LogType.Info);
        }

        /// <summary>
        /// Обработчик события изменение позиции на срочном рынке
        /// </summary>
        /// <param name="futPos"></param>
        private void OnFuturesClientHoldingDo(FuturesClientHolding futures_position)
        {
            Logs.DebugLog("Произошло изменение позиции на срочном рынке", LogType.Info);
        }

        /// <summary>
        /// Обработчик события изменение стакана.
        /// </summary>
        /// <param name="orderbook"></param>
        private void OnQuoteDo(OrderBook orderbook)
        {

        }

        /// <summary>
        /// Обработчик события получения новой свечи.
        /// </summary>
        /// <param name="candle"></param>
        private void OnNewCandle(Candle candle)
        {

        }

        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private void OnStopOrderDo(StopOrder stoporder)
        {

        }

        private void OnTrade(Trade trade)
        {

        }

        private void OnTransReply(TransactionReply reply)
        {

        }

        private void OnParam(Param param)
        {

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
        public async Task<Order> LimitOrder(Tool Tool, Operation operation, decimal price, int vol, string comment)
        {
            try
            {
                price = Math.Round(price, Tool.PriceAccuracy);
                Logs.DebugLog("Выставляем заявку на покупку, по цене:" + price + " ...", LogType.Info);
                long transactionID = (await quik.Orders.SendLimitOrder(Tool.ClassCode, Tool.SecurityCode, Tool.AccountID, operation, price, vol, comment).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    Logs.DebugLog("Заявка выставлена. ID транзакции - " + transactionID, LogType.Info);
                    try
                    {
                        List<Order> Orders = GetOrdersTable();
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tool.ClassCode && order.SecCode == Tool.SecurityCode)
                            {
                                Logs.DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum, LogType.Info);
                                return order;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        Logs.DebugLog("Ошибка получения таблицы заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    Logs.DebugLog("Неудачная попытка выставления заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка выставления заявки.", LogType.Error);
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
        public async Task<Order> MarketOrder(Tool Tool, Operation operation, int vol, string comment)
        {
            try
            {
                //decimal priceInOrder = Math.Round(Tool.LastPrice + Tool.Step * 5, Tool.PriceAccuracy);
                Logs.DebugLog("Выставляем рыночную заявку на покупку...", LogType.Info);
                long transactionID = (await quik.Orders.SendMarketOrder(Tool.ClassCode, Tool.SecurityCode, Tool.AccountID, operation, vol, comment).ConfigureAwait(false)).TransID;
                if (transactionID > 0)
                {
                    Logs.DebugLog("Заявка выставлена. ID транзакции - " + transactionID, LogType.Info);
                    Thread.Sleep(500);
                    try
                    {
                        List<Order> Orders = GetOrdersTable();
                        foreach (Order order in Orders)
                        {
                            if (order.TransID == transactionID && order.ClassCode == Tool.ClassCode && order.SecCode == Tool.SecurityCode)
                            {
                                Logs.DebugLog("Заявка выставлена. Номер заявки - " + order.OrderNum, LogType.Info);
                                //Text2TextBox(textBoxOrderNumber, order.OrderNum.ToString());
                                return order;
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        Logs.DebugLog("Ошибка получения таблицы заявок.", LogType.Error);
                        return null;
                    }
                }
                else
                {
                    Logs.DebugLog("Неудачная попытка выставления заявки.", LogType.Error);
                    return null;
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка выставления заявки.", LogType.Error);
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
            decimal takeprofit, decimal stoplimit, decimal price, Operation operation, int vol, string comment,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
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
                Quantity = vol,
                Comment = comment
            };

            if (operation == Operation.Buy)
                order.Condition = Condition.MoreOrEqual;
            else
                order.Condition = Condition.LessOrEqual;

            return await quik.StopOrders.SendStopOrders(order).ConfigureAwait(false);
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
            decimal takeprofit, decimal price, Operation operation, int vol, string comment,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
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
                Quantity = vol,
                Comment = comment
            };

            if (operation == Operation.Sell)
                order.Condition = Condition.MoreOrEqual;
            else
                order.Condition = Condition.LessOrEqual;

            return await quik.StopOrders.SendStopOrders(order).ConfigureAwait(false);
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
            decimal stoploss, decimal price, Operation operation, int vol, string comment,
            OffsetUnits offset_units = OffsetUnits.PRICE_UNITS, OffsetUnits spread_unit = OffsetUnits.PRICE_UNITS)
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
                Quantity = vol,
                Comment = comment
            };

            if (operation == Operation.Sell)
                order.Condition = Condition.LessOrEqual;
            else
                order.Condition = Condition.MoreOrEqual;
            return await quik.StopOrders.SendStopOrders(order).ConfigureAwait(false);
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
                if (index_order > 0)
                {
                    await quik.Orders.KillOrder(Orders[index_order]);
                    Logs.DebugLog("Заявка №" + Orders[index_order].OrderNum + " снята.", LogType.Info);
                }
                else
                {
                    Logs.DebugLog("Ошибка снятия заявки № " + id_order + ".", LogType.Error);
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка снятия заявки № " + id_order + ".", LogType.Error);
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
                if (index_order > 0)
                {
                    await quik.StopOrders.KillStopOrder(StopOrders[index_order]);
                    Logs.DebugLog("Заявка №" + StopOrders[index_order].OrderNum + " снята.", LogType.Info);
                }
                else
                {
                    Logs.DebugLog("Ошибка снятия заявки № " + id_order + ".", LogType.Error);
                }
            }
            catch
            {
                Logs.DebugLog("Ошибка снятия заявки № " + id_order + ".", LogType.Error);
            }
        }

        #endregion
    }
}
