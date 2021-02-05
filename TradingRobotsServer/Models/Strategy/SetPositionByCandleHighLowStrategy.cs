using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;
using QuikSharp.DataStructures;
using Candle = TradingRobotsServer.Models.Structures.Candle;
using TradingRobotsServer.Models.QuikConnector;
using System.Diagnostics;

namespace TradingRobotsServer.Models.Strategy
{
    public class SetPositionByCandleHighLowStrategy : IStrategy
    {
        public int Window { get; set; }
        public int CandlesViewed { get; set; }
        decimal Indent { get; set; }
        public List<Candle> Candles { get; set; }
        public List<(Candle, Extremum)> Extremums { get; set; }
        public TimeSpan NotTradingTime { get; set; } /* = new TimeSpan(10, 39, 0)*/
        public Operation Operation { get; set; }
        public Bot Bot;
        public bool LookLong;
        public bool LookShort;

        public SetPositionByCandleHighLowStrategy(int window, int candles_viewed, decimal indent, TimeSpan not_trading_time, bool look_long, bool look_short, Bot bot)
        {
            Window = window;
            CandlesViewed = candles_viewed;
            Indent = indent;
            NotTradingTime = not_trading_time;
            Bot = bot;
            LookLong = look_long;
            LookShort = look_short;

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

        void AnalysisCandle(Candle candle)
        {
            if (Candles.Count != 0 && Candles.Count >= Window * 4)
                Candles.RemoveAt(0);
            Candles.Add(candle);

            DebugLog("1 Новая свеча " + candle.ToString());

            FindExtremums();
            CreateDeal();
        }

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
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Close > Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close > Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            return;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Open > Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open > Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Max));
                            return;
                        }
                    }
                }
                if (Candles[index_quotation - 4].Low > Candles[index_quotation - 1].Low & Candles[index_quotation - 3].Low > Candles[index_quotation - 1].Low
                    & Candles[index_quotation - 2].Low > Candles[index_quotation - 1].Low & Candles[index_quotation].Low > Candles[index_quotation - 1].Low)
                {
                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Growing)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Open < Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Open < Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            return;
                        }
                    }

                    if (Candles[index_quotation - 1].TypeCandle == TypeCandle.Falling)
                    {
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Growing && Candles[index_quotation - 1].Close < Candles[index_quotation].Open)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            return;
                        }
                        if (Candles[index_quotation].TypeCandle == TypeCandle.Falling && Candles[index_quotation - 1].Close < Candles[index_quotation].Close)
                        {
                            Extremums.Add((Candles[index_quotation - 1], Extremum.Min));
                            return;
                        }
                    }
                }
            }
        }

        private void CreateDeal()
        {

        }

        void AnalysisTick(Tick tick)
        {
            
        }

        private void GetParam(string param)
        {
            string[] separated_param = param.Split(new char[] { ';' });
            string[] separated_time = separated_param[3].Split(new char[] { ',' });

            Window = Convert.ToInt32(separated_param[0]);
            CandlesViewed = Convert.ToInt32(separated_param[1]);
            Indent = Convert.ToDecimal(separated_param[2]);
            NotTradingTime = new TimeSpan(Convert.ToInt32(separated_time[0]), Convert.ToInt32(separated_time[1]), Convert.ToInt32(separated_time[1]));
            LookLong = Convert.ToBoolean(separated_param[4]);
            LookShort = Convert.ToBoolean(separated_param[5]);
        }

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }

        void IStrategy.AnalysisCandle(Candle candle)
        {
            throw new NotImplementedException();
        }

        void IStrategy.AnalysisTick(Tick tick)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
