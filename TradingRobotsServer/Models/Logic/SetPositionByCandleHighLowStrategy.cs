using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;
using TradingRobotsServer.Models.Logic.Base;
using QuikSharp.DataStructures;
using Candle = TradingRobotsServer.Models.Structures.Candle;
using TradingRobotsServer.Models.QuikConnector;
using System.Diagnostics;
using System.Globalization;
using QuikSharp.DataStructures.Transaction;
using TradingRobotsServer.Models.Support;
using NLog;

namespace TradingRobotsServer.Models.Logic
{
    public class SetPositionByCandleHighLowStrategy : Strategy
    {
        #region Переменные стратегии

        public int Window { get; set; }
        public int CandlesViewed { get; set; }
        decimal Indent { get; set; }
        public List<Candle> Candles { get; set; }
        public List<(Candle, Extremum)> Extremums { get; set; }
        public TimeSpan NotTradingTimeMorning { get; set; } /* = new TimeSpan(10, 39, 0)*/
        public TimeSpan NotTradingTimeNight { get; set; } /* = new TimeSpan(18, 30, 0)*/
        public Operation Operation { get; set; }
        public override Bot Bot { get; set; }
        public bool LookLong;
        public bool LookShort;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private object lock_object = new object();

        public List<DealHighLow> Deals { get; set; }

        #endregion

        #region События

        public override event OnNewOrder NewOrder;

        #endregion

        #region Главные методы

        public SetPositionByCandleHighLowStrategy(int window, int candles_viewed, decimal indent, TimeSpan not_trading_time_morning, TimeSpan not_trading_time_night, bool look_long, bool look_short, Bot bot)
        {
            Window = window;
            CandlesViewed = candles_viewed;
            Indent = indent;
            NotTradingTimeMorning = not_trading_time_morning;
            NotTradingTimeNight = not_trading_time_night;
            LookLong = look_long;
            LookShort = look_short;

            Bot = bot;

            InitialIndicators();
        }
        public SetPositionByCandleHighLowStrategy(string param, Bot bot)
        {
            GetParam(param);
            Bot = bot;

            InitialIndicators();
        }

        private void InitialIndicators()
        {
            Candles = new List<Candle>();
            Extremums = new List<(Candle, Extremum)>();
            Deals = new List<DealHighLow>();
        }

        private void GetParam(string param)
        {
            string[] separated_param = param.Split(new char[] { ';' });
            string[] separated_time_morning = separated_param[3].Split(new char[] { ',' });
            string[] separated_time_night = separated_param[4].Split(new char[] { ',' });

            Window = Convert.ToInt32(separated_param[0]);
            CandlesViewed = Convert.ToInt32(separated_param[1]);
            Indent = Convert.ToDecimal(separated_param[2], new NumberFormatInfo() { NumberDecimalSeparator = "." });
            NotTradingTimeMorning = new TimeSpan(Convert.ToInt32(separated_time_morning[0]), Convert.ToInt32(separated_time_morning[1]), Convert.ToInt32(separated_time_morning[1]));
            NotTradingTimeNight = new TimeSpan(Convert.ToInt32(separated_time_night[0]), Convert.ToInt32(separated_time_night[1]), Convert.ToInt32(separated_time_night[1]));
            LookLong = Convert.ToBoolean(separated_param[5]);
            LookShort = Convert.ToBoolean(separated_param[6]);
        }

        public override void SubsribeNewDeal()
        {
            Bot.NewDeal += OnNewDeal;
        }

        #endregion

        #region Обработка свеч

        public override void AnalysisCandle(Candle candle)
        {
            if (Candles.Count != 0 && Candles.Count >= Window * CandlesViewed)
                Candles.RemoveAt(0);
            Candles.Add(candle);

            bool check = FindExtremums();
            //if (check)
            {
                if (/*DateTime.Now.Minute % 5 == 0 &&*/ !lock_tick_long)
                {
                    lock_tick_long = true;
                    lock (lock_object)
                    {
                        //PlacingOrders(Extremums.Last());
                        PlacingOrdersTemp(candle);
                    }
                }
            }

            CheckDeals(candle);
        }

        private bool FindExtremums()
        {
            int index_quotation = Candles.Count - 1;
            if (index_quotation > 5)
            {
                if (Candles[index_quotation - 4].High < Candles[index_quotation - 1].High && Candles[index_quotation - 3].High < Candles[index_quotation - 1].High
                    && Candles[index_quotation - 2].High <= Candles[index_quotation - 1].High && Candles[index_quotation].High < Candles[index_quotation - 1].High)
                {
                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Growing)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Close >= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close >= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Open >= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open >= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                    }
                }
                if (Candles[index_quotation - 4].Low > Candles[index_quotation - 1].Low & Candles[index_quotation - 3].Low > Candles[index_quotation - 1].Low
                    & Candles[index_quotation - 2].Low >= Candles[index_quotation - 1].Low & Candles[index_quotation].Low > Candles[index_quotation - 1].Low)
                {
                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Growing)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Open <= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open <= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Close <= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close <= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            DebugLog("Найден экстремум: " + Candles[index_quotation - 1].DateTime.ToString() + ", tool: " + Bot.Tool.Name);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void RemoveExtremums()
        {
            if (Extremums[0].Item1.ID < Candles.Count - Window * CandlesViewed)
            {
                Extremums.RemoveAt(0);
            }
        }

        #endregion

        #region Методы для создания и проверки сделок(?)

        // Отправка первичных ордеров.
        public void PlacingOrders((Candle, Extremum) last_extremum)
        {
            decimal price;

            DealHighLow temp_deal = new DealHighLow();
            temp_deal.ID = Deals.Count;

            temp_deal.Status = StatusDeal.WaitingOpen;
            if (last_extremum.Item2 == Extremum.Max)
            {
                temp_deal.Operation = Operation.Buy;
                temp_deal.SignalPoint = new TrendDataPoint(last_extremum.Item1.ID, last_extremum.Item1.High, last_extremum.Item1.DateTime);
                price = last_extremum.Item1.High + Indent;
            }
            else
            {
                return;
                temp_deal.Operation = Operation.Sell;
                temp_deal.SignalPoint = new TrendDataPoint(last_extremum.Item1.ID, last_extremum.Item1.Low, last_extremum.Item1.DateTime);
                price = last_extremum.Item1.High + Indent;
            }
            ((DealHighLow)temp_deal).Distance = ExtremumDistance(last_extremum);

            temp_deal.Vol = -1;
            temp_deal.StopLoss = ((DealHighLow)temp_deal).Distance;
            if (CheckCloneDeal(temp_deal))
                return;

            OrderInfo new_order = new OrderInfo(temp_deal.ID, TypeOrder.StopLimit, Command.SendOrder, price, price, temp_deal.Vol, temp_deal.Operation, "n" + temp_deal.ID + "/Open");

            temp_deal.LogDeal("вход в сделку");

            Deals.Add(temp_deal);

            NewOrder?.Invoke(new_order);
            if (temp_deal.Operation == Operation.Buy)
                lock_tick_long = false;
            if (temp_deal.Operation == Operation.Sell)
                lock_tick_short = false;
            DebugLog("Strategy: Сработало событие нового ордера");
        }

        public void PlacingOrdersTemp(Candle candle)
        {
            decimal price;

            DealHighLow temp_deal = new DealHighLow();
            if (Deals.Count == 0)
                temp_deal.ID = 0;
            else
                temp_deal.ID = Deals.Last().ID + 1;

            temp_deal.Status = StatusDeal.WaitingOpen;

            temp_deal.Operation = Operation.Buy;
            temp_deal.SignalPoint = new TrendDataPoint(candle.ID, candle.High, candle.DateTime);

            price = candle.High + (7m * Bot.Tool.Step);
            ((DealHighLow)temp_deal).Distance = candle.Low - (20m * Bot.Tool.Step);

            temp_deal.Vol = -1;
            temp_deal.StopLoss = ((DealHighLow)temp_deal).Distance;
            if (CheckCloneDeal(temp_deal))
                return;

            OrderInfo new_order = new OrderInfo(temp_deal.ID, TypeOrder.StopLimit, Command.SendOrder, price, price - (3m * Bot.Tool.Step), temp_deal.Vol, temp_deal.Operation, "n" + temp_deal.ID + "/Open");

            temp_deal.LogDeal("вход в сделку");

            Deals.Add(temp_deal);

            NewOrder?.Invoke(new_order);
            if (temp_deal.Operation == Operation.Buy)
                lock_tick_long = false;
            if (temp_deal.Operation == Operation.Sell)
                lock_tick_short = false;
        }

        private bool check_closed_deal = false;
        /// <summary>
        /// Проверка на повторяющиеся сделки.
        /// </summary>
        private bool CheckCloneDeal(Deal deal)
        {
            check_closed_deal = false;

            for (int i = 0; i < Deals.Count; i++)
            {
                if ((Deals[i].Status == StatusDeal.Open || Deals[i].Status == StatusDeal.Closed) && Deals[i].Operation == deal.Operation && Deals[i].SignalPoint.XPoint == deal.SignalPoint.XPoint)
                {
                    check_closed_deal = true;
                }
            }

            return check_closed_deal;
        }

        /// <summary>
        /// Найти последний экстремум.
        /// </summary>
        /// <param name="extremum"></param>
        /// <returns></returns>
        private (Candle, Extremum) FindLastExtremum(Extremum extremum)
        {
            for (int i = Extremums.Count - 1; i >= 0; i--)
            {
                if (Extremums[i].Item2 == extremum)
                {
                    return Extremums[i];
                }
            }

            return (null, Extremum.Null);
        }

        /// <summary>
        /// Найти стоплосс по откату.
        /// </summary>
        /// <param name="deal"></param>
        /// <param name="price"></param>
        /// <param name="count"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public decimal FindStopLossMaxMin(Deal deal, decimal price, int count, Operation operation)
        {
            decimal maxmin = price;
            int point_end = Candles.Last().ID - count;
            if (point_end < 0)
                point_end = 0;

            for (int i = Candles.Last().ID; i >= point_end; i--)
            {
                if (i == deal.SignalPoint.XPoint)
                    break;
                if (operation == Operation.Buy)
                {
                    if (Candles[i].Low < maxmin)
                        maxmin = Candles[i].Low;
                }
                if (operation == Operation.Sell)
                {
                    if (Candles[i].High > maxmin)
                        maxmin = Candles[i].High;
                }
            }

            return maxmin;
        }

        /// <summary>
        /// Рассчет тейк-профитов.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="stoploss"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public decimal CalculationTakeProfits(decimal price, decimal stoploss, Operation operation, decimal multiplier)
        {
            decimal differenceStopLoss = Math.Abs(price - stoploss) / 1.75m;

            if (operation == Operation.Buy)
                return price + differenceStopLoss * multiplier;
            else
                return price - differenceStopLoss * multiplier;
        }

        public Operation ReverseOperation(Operation operation)
        {
            if (operation == Operation.Buy)
                return Operation.Sell;
            else
                return Operation.Buy;
        }

        public decimal ExtremumDistance((Candle, Extremum) extremum)
        {
            int index = extremum.Item1.ID;
            decimal maxmin;
            if (extremum.Item2 == Extremum.Max)
                maxmin = Candles[index].Low;
            else
                maxmin = Candles[index].High;

            for (int i = index; i >= index - 3; i--)
            {
                if (extremum.Item2 == Extremum.Max && Candles[i].Low < maxmin)
                {
                    maxmin = Candles[i].Low;
                }
                else if (extremum.Item2 == Extremum.Min && Candles[i].High > maxmin)
                {
                    maxmin = Candles[i].High;
                }
            }

            if (extremum.Item2 == Extremum.Max)
                return Math.Abs(maxmin);
            else
                return Math.Abs(maxmin);
        }

        public bool CheckDistance(Deal deal, decimal price)
        {
            decimal stoploss = FindStopLossMaxMin(deal, price, Window, deal.Operation);
            decimal distance = Math.Abs(stoploss - deal.SignalPoint.YPoint);
            if (((DealHighLow)deal).Distance < distance)
                return true;
            else
                return false;
        }

        public void CheckDeals(Candle candle)
        {
            for (int i = 0; i < Deals.Count; i++)
            {
                if (Deals[i].Status == StatusDeal.WaitingOpen)
                {
                    if (Deals[i].SignalPoint.XPoint < candle.ID - Window)
                    {
                        CloseDealAndTakeOffOrders(i);

                        Deals[i].LogDeal("закрыта по причине: выхода за пределы окна");
                        Deals[i].ReasonStop = ReasonStop.EndPeriod;
                        return;
                    }

                    if (Deals[i].Operation == Operation.Buy)
                    {
                        if (candle.Low < ((DealHighLow)Deals[i]).Distance)
                        {
                            CloseDealAndTakeOffOrders(i);
                            Deals[i].LogDeal("закрыта по причине: превышения дистанции");
                            Deals[i].ReasonStop = ReasonStop.ExcessDistance;
                            return;
                        }
                    }
                    else
                    {
                        if (candle.High > ((DealHighLow)Deals[i]).Distance)
                        {
                            CloseDealAndTakeOffOrders(i);
                            Deals[i].LogDeal("закрыта по причине: превышения дистанции");
                            Deals[i].ReasonStop = ReasonStop.ExcessDistance;
                            return;
                        }
                    }
                }
            }
        }

        private void CloseDealAndTakeOffOrders(int i)
        {
            Deals[i].Status = StatusDeal.Closed;

            Deals[i].EntryOrder.Command = Command.TakeOffOrder;
            Deals[i].EntryOrder.ExecutionStatus = State.Canceled;
            NewOrder(Deals[i].EntryOrder);
            return;
        }

        #endregion Методы для создания сделки(?)

        #region Контроль и пересчет ордеров

        // Обработка исполненого ордера из Bot.
        public void OnNewDeal(OrderInfo order)
        {
            int index = order.IDDeal;

            switch (order.Command)
            {
                case Command.Null:
                    break;
                case Command.SendOrder:
                    Deals[index].EntryOrder = new OrderInfo(order);
                    break;
                case Command.SendStopLimitOrder:
                    if (Deals[index].FirstStopLossOrder == null)
                    {
                        Deals[index].FirstStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].SecondStopLossOrder == null)
                    {
                        Deals[index].SecondStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].ThirdStopLossOrder == null)
                    {
                        Deals[index].ThirdStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    break;
                case Command.SendTakeProfitOrder:
                    if (Deals[index].FirstTakeProfitOrder == null)
                    {
                        Deals[index].FirstTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].SecondTakeProfitOrder == null)
                    {
                        Deals[index].SecondTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].ThirdTakeProfitOrder == null)
                    {
                        Deals[index].ThirdTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    break;
                case Command.SendTakeProfitAndStopLimitOrder:

                    break;
                case Command.TakeOffOrder:
                    if (Deals[index].EntryOrder.IDOrder == order.IDOrder)
                        Deals[index].EntryOrder = new OrderInfo(order);
                    break;
                case Command.TakeOffStopLimitOrder:
                    if (Deals[index].FirstStopLossOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].FirstStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].SecondStopLossOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].SecondStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].ThirdStopLossOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].ThirdStopLossOrder = new OrderInfo(order);
                        break;
                    }
                    break;
                case Command.TakeOffTakeProfitOrder:
                    if (Deals[index].FirstTakeProfitOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].FirstTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].SecondTakeProfitOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].SecondTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    if (Deals[index].ThirdTakeProfitOrder.IDOrder == order.IDOrder)
                    {
                        Deals[index].ThirdTakeProfitOrder = new OrderInfo(order);
                        break;
                    }
                    break;
                default:
                    break;
            }
        }

        // Фиксирование исполнения ордера.
        public override bool ProcessingExecutedOrders(Trade trade)
        {
            for (int i = 0; i < Deals.Count; i++)
            {
                if (Deals[i].Status == StatusDeal.Closed)
                    continue;

                bool check_order_info = ProcessingExecutedOrders_OrderInfo(trade, i);

                if (check_order_info)
                    return true;

                bool check_stoplimitorder_info = ProcessingExecutedOrders_StopLimitOrdersInfo(trade, i);

                if (check_stoplimitorder_info)
                    return true;

                bool check_takeprofitorder_info = ProcessingExecutedOrders_TakeProfitOrdersInfo(trade, i);

                if (check_takeprofitorder_info)
                    return true;
            }
            return false;
        }

        private bool ProcessingExecutedOrders_OrderInfo(Trade trade, int i)
        {
            {
                //if (trade..OrderNum == Deals[i].EntryOrder.IDOrder && order.State == State.Completed && Deals[i]EntryOrder.ExecutionStatus == State.Active)
                //{
                //    Deals[i]EntryOrder.ExecutionStatus = State.Completed;
                //    Deals[i].Vol = Deals[i]EntryOrder.Vol;
                //    Deals[i].Status = StatusDeal.Open;
                //    Deals[i].TradeEntryPoint = new TrendDataPoint(Candles.Count, order.Price, new DateTime(order.Datetime.year, order.Datetime.month, order.Datetime.day, order.Datetime.hour, order.Datetime.min, order.Datetime.sec));

                //    Deals[i].StopLimitOrdersInfo.AddRange(PlacingStopLimitOrder(Deals[i]));
                //    Deals[i].TakeProfitOrdersInfo.AddRange(PlacingTakeProfitOrder(Deals[i]));

                //    NewOrder?.Invoke(Deals[i], Command.SendStopLimitOrder);
                //    NewOrder?.Invoke(Deals[i], Command.SendTakeProfitOrder);

                //    Debug.WriteLine("Вошли в сделку №" + i);
                //    Deals[i].LogDeal("вход: " + order.Datetime.ToString());
                //    return true;
                //}

                //if (order.OrderNum == Deals[i].OrdersInfo[j].IDLinkedOrder && order.State == State.Completed && Deals[i].OrdersInfo[j].ExecutionLinkedStatus == State.Active)
                //{
                //    Deals[i].OrdersInfo[j].ExecutionLinkedStatus = State.Completed;
                //    Deals[i].Vol = Deals[i].OrdersInfo[j].Vol;
                //    Deals[i].Status = StatusDeal.Open;
                //    Deals[i].TradeEntryPoint = new TrendDataPoint(Candles.Count, order.Price, new DateTime(order.Datetime.year, order.Datetime.month, order.Datetime.day, order.Datetime.hour, order.Datetime.min, order.Datetime.sec));

                //    Deals[i].StopLimitOrdersInfo.AddRange(PlacingStopLimitOrder(Deals[i]));
                //    Deals[i].TakeProfitOrdersInfo.AddRange(PlacingTakeProfitOrder(Deals[i]));

                //    NewOrder?.Invoke(Deals[i], Command.SendStopLimitOrder);
                //    NewOrder?.Invoke(Deals[i], Command.SendTakeProfitOrder);

                //    Debug.WriteLine("Вошли в сделку №" + i);
                //    Deals[i].LogDeal("вход: " + order.Datetime.ToString());
                //    return true;
                //}
            }

            if (trade.OrderNum == Deals[i].EntryOrder.IDLinkedOrder && Deals[i].EntryOrder.ExecutionStatus == State.Active)
            {
                Deals[i].EntryOrder.ExecutionStatus = State.Completed;
                Deals[i].Vol = Deals[i].EntryOrder.Vol;
                Deals[i].Status = StatusDeal.Open;
                Deals[i].TradeEntryPoint = new TrendDataPoint(Candles.Count, (decimal)trade.Price, (DateTime)trade.QuikDateTime);

                NewOrder?.Invoke(PlacingStopLimitOrder(Deals[i]));
                NewOrder?.Invoke(PlacingTakeProfitOrder(Deals[i]));

                Debug.WriteLine("Вошли в сделку №" + i);
                Deals[i].LogDeal("вход: " + trade.QuikDateTime.ToString());
                return true;
            }

            return false;
        }

        private bool ProcessingExecutedOrders_StopLimitOrdersInfo(Trade trade, int i)
        {
            if (trade.OrderNum == Deals[i].FirstStopLossOrder.IDLinkedOrder)
            {
                Deals[i].FirstStopLossOrder.ExecutionStatus = State.Completed;
                Deals[i].Status = StatusDeal.Closed;

                if (Deals[i].FirstTakeProfitOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].FirstTakeProfitOrder.ExecutionStatus = State.Canceled;
                    Deals[i].FirstTakeProfitOrder.Command = Command.TakeOffTakeProfitOrder;
                    NewOrder(Deals[i].FirstTakeProfitOrder);
                }

                Deals[i].ExitPoint = new TrendDataPoint(Candles.Count, (decimal)trade.Price, (DateTime)trade.QuikDateTime);

                Debug.WriteLine("Достигли стоплосса в сделке №" + i);
                Deals[i].ReasonStop = ReasonStop.StopLoss;
                Deals[i].LogDeal("стоплимит: " + trade.QuikDateTime.ToString());
                return true;
            }

            if (trade.OrderNum == Deals[i].SecondStopLossOrder.IDLinkedOrder)
            {
                Deals[i].FirstStopLossOrder.ExecutionStatus = State.Completed;
                Deals[i].Status = StatusDeal.Closed;

                if (Deals[i].SecondTakeProfitOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].SecondTakeProfitOrder.ExecutionStatus = State.Canceled;
                    Deals[i].SecondTakeProfitOrder.Command = Command.TakeOffTakeProfitOrder;
                    NewOrder(Deals[i].SecondTakeProfitOrder);
                }

                Deals[i].ExitPoint = new TrendDataPoint(Candles.Count, (decimal)trade.Price, (DateTime)trade.QuikDateTime);

                Debug.WriteLine("Достигли стоплосса в сделке №" + i);
                Deals[i].ReasonStop = ReasonStop.StopLoss;
                Deals[i].LogDeal("стоплимит: " + trade.QuikDateTime.ToString());
                return true;
            }

            if (trade.OrderNum == Deals[i].ThirdStopLossOrder.IDLinkedOrder)
            {
                Deals[i].FirstStopLossOrder.ExecutionStatus = State.Completed;
                Deals[i].Status = StatusDeal.Closed;

                if (Deals[i].ThirdTakeProfitOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].ThirdTakeProfitOrder.ExecutionStatus = State.Canceled;
                    Deals[i].ThirdTakeProfitOrder.Command = Command.TakeOffTakeProfitOrder;
                    NewOrder(Deals[i].ThirdTakeProfitOrder);
                }

                Deals[i].ExitPoint = new TrendDataPoint(Candles.Count, (decimal)trade.Price, (DateTime)trade.QuikDateTime);

                Debug.WriteLine("Достигли стоплосса в сделке №" + i);
                Deals[i].ReasonStop = ReasonStop.StopLoss;
                Deals[i].LogDeal("стоплимит: " + trade.QuikDateTime.ToString());
                return true;
            }

            return false;
        }

        private bool ProcessingExecutedOrders_TakeProfitOrdersInfo(Trade trade, int i)
        {
            //for (int j = 0; j < Deals[i].TakeProfitOrdersInfo.Count; j++)
            //{
            //    if (order.OrderNum == Deals[i].TakeProfitOrdersInfo[j].IDLinkedOrder && order.State == State.Completed && Deals[i].TakeProfitOrdersInfo[j].ExecutionLinkedStatus == State.Active && Deals[i].Status == StatusDeal.Open)
            //    {
            //        Deals[i].TakeProfitOrdersInfo[j].ExecutionLinkedStatus = State.Completed;

            //        Deals[i].Vol -= Deals[i].TakeProfitOrdersInfo[j].Vol;

            //        for (int k = 0; k < Deals[i].StopLimitOrdersInfo.Count; k++)
            //        {
            //            if (Deals[i].StopLimitOrdersInfo[k].ExecutionStatus != State.Canceled)
            //                Deals[i].StopLimitOrdersInfo[k].ExecutionStatus = State.Canceled;
            //        }

            //        if (Deals[i].Vol == 0)
            //        {
            //            Deals[i].Status = StatusDeal.Closed;
            //            NewOrder(Deals[i], Command.TakeOffStopLimitOrder);
            //            return true;
            //        }

            //        Deals[i].StopLimitOrdersInfo.Add(RecalculateStopLimit(Deals[i]));
            //        Deals[i].TakeProfitOrdersInfo.Add(RecalculateTakeProfit(Deals[i]));

            //        NewOrder(Deals[i], Command.SendTakeProfitOrder);
            //        NewOrder(Deals[i], Command.SendStopLimitOrder);
            //        NewOrder(Deals[i], Command.TakeOffStopLimitOrder);

            //        Debug.WriteLine("Достигли тейк-профита в сделке №" + i);
            //        Deals[i].LogDeal("тейк-профит: " + order.Datetime.ToString());
            //        return true;
            //    }
            //}

            if(trade.OrderNum == Deals[i].FirstTakeProfitOrder.IDLinkedOrder && Deals[i].Status == StatusDeal.Open)
            {
                Deals[i].Vol -= Deals[i].FirstTakeProfitOrder.Vol;

                if (Deals[i].FirstStopLossOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].FirstStopLossOrder.ExecutionStatus = State.Canceled;
                    Deals[i].FirstStopLossOrder.Command = Command.TakeOffStopLimitOrder;
                    NewOrder(Deals[i].FirstStopLossOrder);
                }

                NewOrder(RecalculateStopLimit(Deals[i]));
                NewOrder(RecalculateTakeProfit(Deals[i]));

                Debug.WriteLine("Достигли тейк-профита в сделке №" + i);
                Deals[i].LogDeal("тейк-профит: " + trade.QuikDateTime.ToString());
                return true;
            }


            if (trade.OrderNum == Deals[i].SecondTakeProfitOrder.IDLinkedOrder && Deals[i].Status == StatusDeal.Open)
            {
                Deals[i].Vol -= Deals[i].SecondTakeProfitOrder.Vol;

                if (Deals[i].SecondStopLossOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].SecondStopLossOrder.ExecutionStatus = State.Canceled;
                    Deals[i].SecondStopLossOrder.Command = Command.TakeOffStopLimitOrder;
                    NewOrder(Deals[i].SecondStopLossOrder);
                }

                NewOrder(RecalculateStopLimit(Deals[i]));
                NewOrder(RecalculateTakeProfit(Deals[i]));

                Debug.WriteLine("Достигли тейк-профита в сделке №" + i);
                Deals[i].LogDeal("тейк-профит: " + trade.QuikDateTime.ToString());
                return true;
            }


            if (trade.OrderNum == Deals[i].ThirdTakeProfitOrder.IDLinkedOrder && Deals[i].Status == StatusDeal.Open)
            {
                Deals[i].Vol -= Deals[i].ThirdTakeProfitOrder.Vol;

                if (Deals[i].ThirdStopLossOrder.ExecutionStatus != State.Canceled)
                {
                    Deals[i].ThirdStopLossOrder.ExecutionStatus = State.Canceled;
                    Deals[i].ThirdStopLossOrder.Command = Command.TakeOffStopLimitOrder;
                    NewOrder(Deals[i].ThirdStopLossOrder);
                }

                NewOrder(RecalculateStopLimit(Deals[i]));
                NewOrder(RecalculateTakeProfit(Deals[i]));

                Debug.WriteLine("Достигли тейк-профита в сделке №" + i);
                Deals[i].LogDeal("тейк-профит: " + trade.QuikDateTime.ToString());
                return true;
            }

            return false;
        }

        // Фиксирование исполнения стоп-ордера.
        public override bool ProcessingExecutedStopOrders(StopOrder stoporder)
        {
            for (int i = 0; i < Deals.Count; i++)
            {
                //стоп-заявка на вход
                if (stoporder.OrderNum == Deals[i].EntryOrder.IDOrder && Deals[i].EntryOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].EntryOrder.ExecutionStatus = State.Completed;
                    Deals[i].Vol = Deals[i].EntryOrder.Vol;
                    Deals[i].EntryOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Вошли в сделку стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка на вход: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка первого стоплосса
                if (stoporder.OrderNum == Deals[i].FirstStopLossOrder.IDOrder && Deals[i].FirstStopLossOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].FirstStopLossOrder.ExecutionStatus = State.Completed;
                    Deals[i].StopLoss = Deals[i].FirstStopLossOrder.Price;
                    Deals[i].FirstStopLossOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли стоплосса стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка стоплимит: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка второго стоплосса
                if (stoporder.OrderNum == Deals[i].SecondStopLossOrder.IDOrder && Deals[i].SecondStopLossOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].SecondStopLossOrder.ExecutionStatus = State.Completed;
                    Deals[i].StopLoss = Deals[i].SecondStopLossOrder.Price;
                    Deals[i].SecondStopLossOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли стоплосса стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка стоплимит: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка третьего стоплосса
                if (stoporder.OrderNum == Deals[i].ThirdStopLossOrder.IDOrder && Deals[i].ThirdStopLossOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].ThirdStopLossOrder.ExecutionStatus = State.Completed;
                    Deals[i].StopLoss = Deals[i].ThirdStopLossOrder.Price;
                    Deals[i].ThirdStopLossOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли стоплосса стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка стоплимит: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка первого тейка
                if (stoporder.OrderNum == Deals[i].FirstTakeProfitOrder.IDOrder && Deals[i].FirstTakeProfitOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].FirstTakeProfitOrder.ExecutionStatus = State.Completed;
                    Deals[i].FirstTakeProfitOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли тейк-профита стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка тейк-профит: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка второго тейка
                if (stoporder.OrderNum == Deals[i].SecondTakeProfitOrder.IDOrder && Deals[i].SecondTakeProfitOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].SecondTakeProfitOrder.ExecutionStatus = State.Completed;
                    Deals[i].SecondTakeProfitOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли тейк-профита стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка тейк-профит: " + DateTime.Now);
                    return true;
                }
                //стоп-заявка третьего тейка
                if (stoporder.OrderNum == Deals[i].ThirdTakeProfitOrder.IDOrder && Deals[i].ThirdTakeProfitOrder.ExecutionStatus == State.Active && stoporder.LinkedOrder > 0)
                {
                    Deals[i].ThirdTakeProfitOrder.ExecutionStatus = State.Completed;
                    Deals[i].ThirdTakeProfitOrder.IDLinkedOrder = stoporder.LinkedOrder;

                    Debug.WriteLine("Достигли тейк-профита стоп-заявкой №" + i + " linkorder: " + stoporder.LinkedOrder);
                    Deals[i].LogDeal("стоп-заявка тейк-профит: " + DateTime.Now);
                    return true;
                }
            }
            return false;
        }

        // Рассчет первичных стоп-лимитов.
        public override OrderInfo PlacingStopLimitOrder(Deal deal)
        {
            decimal stoploss = ((DealHighLow)deal).EntryOrder.Price3 - (50m * Bot.Tool.Step) /*FindStopLossMaxMin(deal, deal.OrdersInfo[0].Price3, Window, deal.Operation)*/;
            DebugLog("Strategy: Сработало событие нового стоп-лимита");

            return new OrderInfo(deal.ID, TypeOrder.StopLimit, Command.SendStopLimitOrder, stoploss, stoploss - (5m * Bot.Tool.Step), -1, ReverseOperation(deal.Operation), "n" + deal.ID + "/StopLoss" + 0);
        }

        // Рассчет первичных тейк-профитов.
        public override OrderInfo PlacingTakeProfitOrder(Deal deal)
        {
            decimal profit = CalculationTakeProfits(((DealHighLow)deal).EntryOrder.Price3, deal.StopLoss, deal.Operation, 1m);

            DebugLog("Strategy: Сработало событие нового стоп-ордера");

            return new OrderInfo(deal.ID, TypeOrder.TakeProfit, Command.SendTakeProfitOrder, ((DealHighLow)deal).EntryOrder.Price + (20m * Bot.Tool.Step), -1,
                ReverseOperation(deal.Operation), "n" + deal.ID + "/TakeProfit" + 0);
        }

        // Пересчет стоп-лимитов.
        public override OrderInfo RecalculateStopLimit(Deal deal)
        {
            if (((DealHighLow)deal).SecondStopLossOrder == null && ((DealHighLow)deal).FirstStopLossOrder.ExecutionStatus == State.Canceled)
            {
                decimal stoploss = ((DealHighLow)deal).EntryOrder.Price;
                return new OrderInfo(deal.ID, TypeOrder.StopLimit, Command.SendStopLimitOrder, stoploss, stoploss + (2m * Bot.Tool.Step), deal.Vol, ReverseOperation(deal.Operation), "n" + deal.ID + "/StopLoss" + 1);
            }
            if (((DealHighLow)deal).ThirdStopLossOrder == null && ((DealHighLow)deal).SecondStopLossOrder.ExecutionStatus == State.Canceled)
            {
                decimal stoploss = ((DealHighLow)deal).FirstTakeProfitOrder.Price;
                return new OrderInfo(deal.ID, TypeOrder.StopLimit, Command.SendStopLimitOrder, stoploss, stoploss + (2m * Bot.Tool.Step), deal.Vol, ReverseOperation(deal.Operation), "n" + deal.ID + "/StopLoss" + 2);
            }
            //if (deal.TakeProfitOrdersInfo.Count == 3 && deal.TakeProfitOrdersInfo[2].ExecutionStatus == State.Completed)
            //{
            //    decimal stoploss = deal.TakeProfitOrdersInfo[1].Price;
            //    return new OrderInfo(TypeOrder.StopLimit, stoploss, stoploss - (2m * Bot.Tool.Step), deal.Vol, ReverseOperation(deal.Operation), State.Active, State.Active);
            //}
            return null;
        }

        // Пересчет тейк-профитов.
        public override OrderInfo RecalculateTakeProfit(Deal deal)
        {
            if (((DealHighLow)deal).SecondTakeProfitOrder == null && ((DealHighLow)deal).FirstTakeProfitOrder.ExecutionStatus == State.Canceled)
            {
                //decimal profit = CalculationTakeProfits(deal.OrdersInfo[0].Price, deal.StopLoss, deal.Operation, 2m);
                return new OrderInfo(deal.ID, TypeOrder.TakeProfit, Command.SendTakeProfitOrder, ((DealHighLow)deal).EntryOrder.Price + (20m * Bot.Tool.Step), -1, ReverseOperation(deal.Operation), "n" + deal.ID + "/TakeProfit" + 1);
            }
            if (((DealHighLow)deal).ThirdTakeProfitOrder == null && ((DealHighLow)deal).SecondTakeProfitOrder.ExecutionStatus == State.Canceled)
            {
                //decimal profit = CalculationTakeProfits(deal.OrdersInfo[0].Price, deal.StopLoss, deal.Operation, 3m);
                return new OrderInfo(deal.ID, TypeOrder.TakeProfit, Command.SendTakeProfitOrder, ((DealHighLow)deal).EntryOrder.Price + (40m * Bot.Tool.Step), deal.Vol, ReverseOperation(deal.Operation), "n" + deal.ID + "/TakeProfit" + 2);
            }
            return null;
        }

        #endregion Контроль и пересчет ордеров

        #region Обработка тиков

        DateTime temp_time = new DateTime(2021, 2, 16, 13, 26, 0);
        private bool lock_tick_long = false;
        private bool lock_tick_short = false;

        //
        public override void AnalysisTick(Tick tick)
        {

        }

        #endregion Обработка тиков

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
            logger.Info(log_string);
        }

        #endregion Лог
    }
}
