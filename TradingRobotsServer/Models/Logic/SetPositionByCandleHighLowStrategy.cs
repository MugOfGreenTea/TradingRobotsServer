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

namespace TradingRobotsServer.Models.Logic
{
    public class SetPositionByCandleHighLowStrategy : Strategy
    {
        public int Window { get; set; }
        public int CandlesViewed { get; set; }
        decimal Indent { get; set; }
        public List<Candle> Candles { get; set; }
        public List<(Candle, Extremum)> Extremums { get; set; }
        public TimeSpan NotTradingTimeMorning { get; set; } /* = new TimeSpan(10, 39, 0)*/
        public TimeSpan NotTradingTimeNight { get; set; } /* = new TimeSpan(18, 30, 0)*/
        public Operation Operation { get; set; }
        public Bot Bot;
        public bool LookLong;
        public bool LookShort;

        public override event OnNewOrder NewOrder;

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
        }

        public override void AnalysisCandle(Candle candle)
        {
            if (Candles.Count != 0 && Candles.Count >= Window * CandlesViewed)
                Candles.RemoveAt(0);
            Candles.Add(candle);

            DebugLog("1 Новая свеча " + candle.ToString());

            FindExtremums();
            CreateDeal();
        }

        private decimal level_last_extremum_max;
        private decimal level_last_extremum_min;

        private void FindExtremums()
        {
            int index_quotation = Candles.Count - 1;
            if (index_quotation > 5)
            {
                if (Candles[index_quotation - 4].High < Candles[index_quotation - 1].High && Candles[index_quotation - 3].High < Candles[index_quotation - 1].High
                    && Candles[index_quotation - 2].High < Candles[index_quotation - 1].High && Candles[index_quotation].High < Candles[index_quotation - 1].High)
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
                    & Candles[index_quotation - 2].Low > Candles[index_quotation - 1].Low & Candles[index_quotation].Low > Candles[index_quotation - 1].Low)
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

        private void CreateDeal()
        {

        }

        public override void AnalysisTick(Tick tick)
        {
            if (Candles.Count == 0)
                return;

            DateTime temp_nottradingtimemorning = new DateTime(Candles.Last().DateTime.Year, Candles.Last().DateTime.Month, Candles.Last().DateTime.Day, NotTradingTimeMorning.Hours, NotTradingTimeMorning.Minutes, NotTradingTimeMorning.Seconds);
            DateTime temp_nottradingtimenight = new DateTime(Candles.Last().DateTime.Year, Candles.Last().DateTime.Month, Candles.Last().DateTime.Day, NotTradingTimeNight.Hours, NotTradingTimeNight.Minutes, NotTradingTimeNight.Seconds);
            if (Candles.Last().DateTime <= temp_nottradingtimemorning)
                return;
            if (Candles.Last().DateTime >= temp_nottradingtimenight)
                return;
            if (Extremums.Count == 0)
                return;

            (Candle, Extremum) last_extremum = FindLastExtremum(Extremum.Max);

            if (last_extremum.Item2 == Extremum.Max && last_extremum.Item1.ID > Candles.Count - Window)
            {
                if (tick.Price > last_extremum.Item1.High + Indent)
                {
                    PlacingOrders(last_extremum, tick.Price, Operation.Buy);
                }
            }
            else if (last_extremum.Item2 == Extremum.Min && last_extremum.Item1.ID > Candles.Count - Window)
            {
                if (tick.Price < last_extremum.Item1.Low - Indent)
                {
                    PlacingOrders(last_extremum, tick.Price, Operation.Sell);
                }
            }
        }

        #region

        private void PlacingOrders((Candle, Extremum) last_extremum, decimal price, Operation operation)
        {
            Deal temp_deal = new Deal();
            temp_deal.SignalPoint = new TrendDataPoint(-1, last_extremum.Item1.High, last_extremum.Item1.DateTime);
            temp_deal.Status = StatusDeal.WaitingOpen;
            temp_deal.Operation = operation;

            List<OrderInfo> new_order = new List<OrderInfo>();
            new_order.Add(new OrderInfo(TypeOrder.LimitOrder, price, 3, operation));

            decimal stoploss = FindStopLossMaxMin(price, Window, operation);
            new_order.Add(new OrderInfo(TypeOrder.StopLimit, stoploss, 3, ReverseEnum(operation)));

            (decimal, decimal, decimal) profits = CalculationTakeProfits(price, stoploss, operation);
            new_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item1, 1, ReverseEnum(operation)));
            new_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item2, 1, ReverseEnum(operation)));
            new_order.Add(new OrderInfo(TypeOrder.TakeProfit, profits.Item3, 1, ReverseEnum(operation)));

            temp_deal.Orders.AddRange(new_order);

            NewOrder?.Invoke(temp_deal);
            Debug.WriteLine("Сработало событие нового ордера");
            Debug.WriteLine("price: " + price);
            Debug.WriteLine("stoploss: " + stoploss);
            Debug.WriteLine("takeprofit 1: " + profits.Item1);
            Debug.WriteLine("takeprofit 2: " + profits.Item2);
            Debug.WriteLine("takeprofit 3: " + profits.Item3);
        }

        private (Candle, Extremum) FindLastExtremum(Extremum extremum)
        {
            for (int i = Extremums.Count-1; i >= 0; i--)
            {
                if(Extremums[i].Item2 == extremum)
                {
                    return Extremums[i];
                }
            }

            return (null, Extremum.Null);
        }

        public decimal FindStopLossMaxMin(decimal price, int count, Operation operation)
        {
            decimal maxmin = price;
            int point_end = Candles.Last().ID - count;
            if (point_end < 0)
                point_end = 0;

            for (int i = Candles.Last().ID - 1; i > point_end; i--)
            {
                if (i == Extremums.Last().Item1.ID)
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

        public T ReverseEnum<T>(T enum_value)
        {
            if ((object)enum_value == (object)0)
                return (T)(object)1;
            else
                return (T)(object)0;
        }

        #endregion

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

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }

        #endregion
    }
}
