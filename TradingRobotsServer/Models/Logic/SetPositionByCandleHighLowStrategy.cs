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

        private List<Deal> Deals { get; set; }

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
            Deals = new List<Deal>();
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

            //DebugLog("1 Новая свеча " + candle.ToString());

            FindExtremums();
        }

        private void FindExtremums()
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
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close >= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Open >= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open >= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            Debug.WriteLine("Найден экстремум");
                            return;
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
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open <= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Close <= Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close <= Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            Debug.WriteLine("Найден экстремум");
                            return;
                        }
                    }
                }
            }
        }

        private void RemoveExtremums()
        {
            if (Extremums[0].Item1.ID < Candles.Count - Window * CandlesViewed)
            {
                Extremums.RemoveAt(0);
            }
        }

        #endregion

        #region Обработка тиков

        DateTime temp_time = new DateTime(2021, 2, 12, 15, 39, 0);
        //Создание сделки и отправка обычной заявки
        public override void AnalysisTick(Tick tick)
        {
            if (Candles.Count == 0)
                return;

            #region
            DateTime temp_nottradingtimemorning = new DateTime(Candles.Last().DateTime.Year, Candles.Last().DateTime.Month, Candles.Last().DateTime.Day, NotTradingTimeMorning.Hours, NotTradingTimeMorning.Minutes, NotTradingTimeMorning.Seconds);
            DateTime temp_nottradingtimenight = new DateTime(Candles.Last().DateTime.Year, Candles.Last().DateTime.Month, Candles.Last().DateTime.Day, NotTradingTimeNight.Hours, NotTradingTimeNight.Minutes, NotTradingTimeNight.Seconds);
            if (Candles.Last().DateTime <= temp_nottradingtimemorning)
                return;
            if (Candles.Last().DateTime >= temp_nottradingtimenight)
                return;
            if (Extremums.Count == 0)
                return;

            (Candle, Extremum) last_extremum = FindLastExtremum(Extremum.Max);

            if (last_extremum.Item2 == Extremum.Max && last_extremum.Item1.ID > Candles.Last().ID - Window)
            {
                if (tick.Price > last_extremum.Item1.High + Indent)
                {
                    Debug.WriteLine("Попытка открыть сделку в LONG");
                    PlacingOrders(last_extremum, tick.Price, Operation.Buy);
                }
            }
            else if (last_extremum.Item2 == Extremum.Min && last_extremum.Item1.ID > Candles.Last().ID - Window)
            {
                if (tick.Price < last_extremum.Item1.Low - Indent)
                {
                    Debug.WriteLine("Попытка открыть сделку в SHORT");
                    PlacingOrders(last_extremum, tick.Price, Operation.Sell);
                }
            }
            #endregion
            //if (DateTime.Now.Minute == temp_time.Minute)
            //{
            //    PlacingOrderTemp(tick.Price, Operation.Buy, temp_time);
            //}
        }

        #region Отправка и пересчет ордеров

        // Отправка первичных ордеров.
        public override void PlacingOrders((Candle, Extremum) last_extremum, decimal price, Operation operation)
        {
            Deal temp_deal = new DealHighLow();
            temp_deal.ID = Deals.Count;

            temp_deal.SignalPoint = new TrendDataPoint(-1, last_extremum.Item1.High, last_extremum.Item1.DateTime);
            temp_deal.Status = StatusDeal.WaitingOpen;
            temp_deal.Operation = operation;
            ((DealHighLow)temp_deal).Distance = ExtremumDistance(last_extremum);

            temp_deal.Vol += 3;

            if (!CheckDeal(temp_deal))
                return;
            if (!CheckDistance(temp_deal, price))
                return;

            List<OrderInfo> new_order = new List<OrderInfo>();
            new_order.Add(new OrderInfo(TypeOrder.LimitOrder, price, 3, operation, State.Active, State.Active));

            temp_deal.OrdersInfo.AddRange(new_order);

            NewOrder?.Invoke(temp_deal, Command.SendOrder);
            Debug.WriteLine("Strategy: Сработало событие нового ордера");
        }

        private void PlacingOrderTemp(decimal price, Operation operation, DateTime time)
        {
            Deal temp_deal = new Deal();
            temp_deal.ID = Deals.Count;

            temp_deal.SignalPoint = new TrendDataPoint(-1, 270, time);
            temp_deal.Status = StatusDeal.WaitingOpen;
            temp_deal.Operation = operation;
            temp_deal.Vol += 3;

            if (!CheckDeal(temp_deal))
                return;

            price -= 0.02m;

            List<OrderInfo> new_order = new List<OrderInfo>();
            new_order.Add(new OrderInfo(TypeOrder.LimitOrder, price, 3, operation, State.Active, State.Active));

            temp_deal.OrdersInfo.AddRange(new_order);

            NewOrder?.Invoke(temp_deal, Command.SendOrder);
            Debug.WriteLine("Strategy: Сработало событие нового ордера");
        }

        // Обработка новой сделки из Bot.
        public void OnNewDeal(Deal deal, Command command)
        {
            if (Deals.Count == deal.ID)
            {
                deal.Status = StatusDeal.Open;
                Deals.Add(deal); // новая сделка
            }
            else
            {
                int index = deal.ID;

                switch (command)
                {
                    case Command.Null:
                        break;
                    case Command.SendOrder:
                        Deals[index].OrdersInfo = deal.OrdersInfo;
                        break;
                    case Command.SendStopLimitOrder:
                        Deals[index].StopLimitOrdersInfo = deal.StopLimitOrdersInfo;
                        break;
                    case Command.SendTakeProfitOrder:
                        Deals[index].TakeProfitOrdersInfo = deal.TakeProfitOrdersInfo;
                        break;
                    case Command.SendTakeProfitAndStopLimitOrder:
                        Deals[index].TakeProfitAndStopLimitOrdersInfo = deal.TakeProfitAndStopLimitOrdersInfo;
                        break;
                    case Command.TakeOffOrder:
                        Deals[index].OrdersInfo = deal.OrdersInfo;
                        break;
                    case Command.TakeOffStopLimitOrder:
                        Deals[index].StopLimitOrdersInfo = deal.StopLimitOrdersInfo;
                        break;
                    case Command.TakeOffTakeProfitOrder:
                        Deals[index].TakeProfitOrdersInfo = deal.TakeProfitOrdersInfo;
                        break;
                    default:
                        break;
                }
            }
        }

        // Фиксирование исполнения ордера.
        public override void ProcessingExecutedOrders(Order order)
        {
            for (int i = 0; i < Deals.Count; i++)
            {
                for (int j = 0; j < Deals[i].OrdersInfo.Count; j++)
                {
                    if (order.OrderNum == Deals[i].OrdersInfo[j].IDOrder && order.State == State.Completed && Deals[i].OrdersInfo[j].ExecutionStatus == State.Active)
                    {
                        Deals[i].OrdersInfo[j].ExecutionStatus = State.Completed;
                        Deals[i].StopLimitOrdersInfo.AddRange(PlacingStopLimitOrder(Deals[i]));
                        Deals[i].TakeProfitOrdersInfo.AddRange(PlacingTakeProfitOrder(Deals[i]));

                        NewOrder?.Invoke(Deals[i], Command.SendStopLimitOrder);
                        NewOrder?.Invoke(Deals[i], Command.SendTakeProfitOrder);

                        return;
                    }
                }

                for (int j = 0; j < Deals[i].StopLimitOrdersInfo.Count; j++)
                {
                    if (order.OrderNum == Deals[i].StopLimitOrdersInfo[j].IDLinkedOrder && order.State == State.Completed && Deals[i].Status == StatusDeal.Open)
                    {
                        Deals[i].Status = StatusDeal.Closed;

                        for (int k = 0; k < Deals[i].TakeProfitOrdersInfo.Count; k++)
                        {
                            if (Deals[i].TakeProfitOrdersInfo[k].ExecutionStatus == State.Active)
                                Deals[i].TakeProfitOrdersInfo[k].ExecutionStatus = State.Canceled;
                        }

                        NewOrder(Deals[i], Command.TakeOffTakeProfitOrder);

                        return;
                    }
                }

                for (int j = 0; j < Deals[i].TakeProfitOrdersInfo.Count; j++)
                {
                    if (order.OrderNum == Deals[i].TakeProfitOrdersInfo[j].IDLinkedOrder && order.State == State.Completed && Deals[i].TakeProfitOrdersInfo[j].ExecutionLinkedStatus == State.Active)
                    {
                        Deals[i].TakeProfitOrdersInfo[j].ExecutionLinkedStatus = State.Completed;

                        Deals[i].Vol -= 1;
                        Deals[i].StopLimitOrdersInfo.Last().ExecutionStatus = State.Canceled;
                        Deals[i].StopLimitOrdersInfo.Add(RecalculateStopLimit(Deals[i]));

                        NewOrder(Deals[i], Command.TakeOffStopLimitOrder);
                        NewOrder(Deals[i], Command.SendStopLimitOrder);

                        if (Deals[i].Vol == 0)
                            Deals[i].Status = StatusDeal.Closed;

                        return;
                    }
                }
            }
        }

        // Фиксирование исполнения стоп-ордера.
        public override void ProcessingExecutedStopOrders(StopOrder stoporder)
        {
            for (int i = 0; i < Deals.Count; i++)
            {
                for (int j = 0; j < Deals[i].StopLimitOrdersInfo.Count; j++)
                {
                    if (stoporder.OrderNum == Deals[i].StopLimitOrdersInfo[j].IDOrder && stoporder.State == State.Completed && Deals[i].StopLimitOrdersInfo[j].ExecutionStatus == State.Active)
                    {
                        Deals[i].StopLimitOrdersInfo[j].ExecutionStatus = State.Completed;
                        Deals[i].StopLimitOrdersInfo[j].IDLinkedOrder = stoporder.LinkedOrder;

                        return;
                    }
                }

                for (int j = 0; j < Deals[i].TakeProfitOrdersInfo.Count; j++)
                {
                    if (stoporder.OrderNum == Deals[i].TakeProfitOrdersInfo[j].IDOrder && stoporder.State == State.Completed && Deals[i].TakeProfitOrdersInfo[j].ExecutionStatus == State.Active)
                    {
                        Deals[i].TakeProfitOrdersInfo[j].ExecutionStatus = State.Completed;
                        Deals[i].TakeProfitOrdersInfo[j].ExecutionLinkedStatus = State.Active;
                        Deals[i].TakeProfitOrdersInfo[j].IDLinkedOrder = stoporder.LinkedOrder;

                        return;
                    }
                }
            }
        }

        // Рассчет первичных стоп-лимитов.
        public override List<OrderInfo> PlacingStopLimitOrder(Deal deal)
        {
            List<OrderInfo> new_stop_order = new List<OrderInfo>();

            decimal stoploss = FindStopLossMaxMin(deal, deal.OrdersInfo[0].Price, Window, deal.Operation);
            new_stop_order.Add(new OrderInfo(TypeOrder.StopLimit, stoploss, stoploss - 0.02m, 3, ReverseOperation(deal.Operation), State.Active, State.Active));
            Debug.WriteLine("Strategy: Сработало событие нового стоп-лимита");

            return new_stop_order;
        }

        // Рассчет первичных тейк-профитов.
        public override List<OrderInfo> PlacingTakeProfitOrder(Deal deal)
        {
            List<OrderInfo> new_stop_order = new List<OrderInfo>();
            (decimal, decimal, decimal) profits = CalculationTakeProfits(deal.OrdersInfo[0].Price, deal.StopLimitOrdersInfo[0].Price, deal.Operation);
            new_stop_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item1, 1, ReverseOperation(deal.Operation), State.Active, State.Active));
            new_stop_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item2, 1, ReverseOperation(deal.Operation), State.Active, State.Active));
            new_stop_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item3, 1, ReverseOperation(deal.Operation), State.Active, State.Active));

            Debug.WriteLine("Strategy: Сработало событие нового стоп-ордера");

            return new_stop_order;
        }

        // Пересчет стоп-лимитов.
        public override OrderInfo RecalculateStopLimit(Deal deal)
        {
            if (deal.TakeProfitOrdersInfo[0].ExecutionStatus == State.Completed && deal.TakeProfitOrdersInfo[1].ExecutionStatus == State.Active && deal.TakeProfitOrdersInfo[2].ExecutionStatus == State.Active)
            {
                decimal stoploss = deal.OrdersInfo[0].Price;
                return new OrderInfo(TypeOrder.StopLimit, stoploss, stoploss - 0.02m, deal.Vol, ReverseOperation(deal.Operation), State.Active, State.Active);
            }
            if (deal.TakeProfitOrdersInfo[0].ExecutionStatus == State.Completed && deal.TakeProfitOrdersInfo[1].ExecutionStatus == State.Completed && deal.TakeProfitOrdersInfo[2].ExecutionStatus == State.Active)
            {
                decimal stoploss = deal.TakeProfitOrdersInfo[0].Price;
                return new OrderInfo(TypeOrder.StopLimit, stoploss, stoploss - 0.02m, deal.Vol, ReverseOperation(deal.Operation), State.Active, State.Active);
            }
            if (deal.TakeProfitOrdersInfo[0].ExecutionStatus == State.Completed && deal.TakeProfitOrdersInfo[1].ExecutionStatus == State.Completed && deal.TakeProfitOrdersInfo[2].ExecutionStatus == State.Completed)
            {
                decimal stoploss = deal.TakeProfitOrdersInfo[1].Price;
                return new OrderInfo(TypeOrder.StopLimit, stoploss, stoploss - 0.02m, deal.Vol, ReverseOperation(deal.Operation), State.Active, State.Active);
            }
            return null;
        }

        // Пересчет тейк-профитов.
        public override OrderInfo RecalculateTakeProfit(Deal deal)
        {


            return null;
        }

        #endregion Отправка и пересчет ордеров

        #region Методы для создания сделки(?)

        private bool check_closed_deal = true;
        /// <summary>
        /// Проверка на повторяющиеся сделки.
        /// </summary>
        private bool CheckDeal(Deal deal)
        {
            check_closed_deal = true;

            for (int i = 0; i < Deals.Count; i++)
            {
                if ((Deals[i].Status == StatusDeal.Open || Deals[i].Status == StatusDeal.Closed) && Deals[i].Operation == deal.Operation && deal.SignalPoint.XPoint == Deals[i].SignalPoint.XPoint)
                {
                    check_closed_deal = false;
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

            for (int i = Candles.Last().ID - 1; i > point_end; i--)
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
        public (decimal, decimal, decimal) CalculationTakeProfits(decimal price, decimal stoploss, Operation operation)
        {
            decimal differenceStopLoss = Math.Abs(price - stoploss) / 1.75m;

            if (operation == Operation.Buy)
            {
                decimal FirstTakeProfit = price + differenceStopLoss;
                decimal SecondTakeProfit = price + differenceStopLoss * 2;
                decimal ThirdTakeProfit = price + differenceStopLoss * 3;

                return (FirstTakeProfit, SecondTakeProfit, ThirdTakeProfit);
            }
            else
            {
                decimal FirstTakeProfit = price - differenceStopLoss;
                decimal SecondTakeProfit = price - differenceStopLoss * 2;
                decimal ThirdTakeProfit = price - differenceStopLoss * 3;

                return (FirstTakeProfit, SecondTakeProfit, ThirdTakeProfit);
            }
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

            for (int i = index - 1; i >= index - 3; i--)
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
                return Math.Abs(extremum.Item1.High - maxmin);
            else
                return Math.Abs(extremum.Item1.Low - maxmin);
        }

        public bool CheckDistance(Deal deal, decimal price)
        {
            decimal stoploss = FindStopLossMaxMin(deal, price, Window, deal.Operation);
            decimal distance = Math.Abs(stoploss - deal.SignalPoint.YPoint);
            if (((DealHighLow)deal).Distance < distance)
                return false;
            else
                return true;
        }

        #endregion Методы для создания сделки(?)

        #endregion Обработка тиков

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }

        #endregion Лог
    }

    public class DealHighLow : Deal
    {
        public decimal Distance { get; set; }
    }
}
