using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;
using QuikSharp.DataStructures;
using Candle = QuikSharp.DataStructures.Candle;
using TradingRobotsServer.Models.QuikConnector;
using System.Diagnostics;

namespace TradingRobotsServer.Models.Strategy
{
    public class SetPositionByCandleHighLowStrategy
    {
        public int Window { get; set; }
        public int CandlesViewed { get; set; }
        decimal Indent { get; set; }
        public List<Candle> Candles { get; set; }
        public List<(Candle, Extremum)> Extremums { get; set; }
        public TimeSpan NotTradingTime { get; set; } /* = new TimeSpan(10, 39, 0)*/
        public Operation Operation { get; set; }

        public delegate void OnNewCandle(Candle candle);
        public event OnNewCandle NewCandle;
        public delegate void OnNewTick(decimal last_price);
        public event OnNewTick NewTick;

        public unsafe SetPositionByCandleHighLowStrategy(int window, int candles_viewed,  decimal indent, TimeSpan not_trading_time)
        {
            Window = window;
            CandlesViewed = candles_viewed;
            Indent = indent;
            NotTradingTime = not_trading_time;

            InitialIndicators();

            NewCandle += AnalysisCandle;
            NewTick += AnalysisTick;
        }

        private void InitialIndicators()
        {
            Candles = new List<Candle>();
            Extremums = new List<(Candle, Extremum)>();
        }

        private void AnalysisCandle(Candle candle)
        {
            if (Candles.Count != 0)
                Candles.RemoveAt(0);
            Candles.Add(candle);

            FindExtremums();
            CreateDeal();
        }

        private void FindExtremums()
        {

        }

        private void CreateDeal()
        {

        }

        public void AnalysisTick(decimal last_price)
        {

        }

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }

        #endregion
    }
}
