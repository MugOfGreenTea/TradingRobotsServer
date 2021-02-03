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
        public List<(Candle, Extremum)> Extremums { get; set; }
        public TimeSpan NotTradingTime { get; set; } /* = new TimeSpan(10, 39, 0)*/
        public Operation Operation { get; set; }

        public Tool Tool { get; set; }

        public SetPositionByCandleHighLowStrategy(int window, int candles_viewed,  decimal indent, TimeSpan not_trading_time, Tool tool)
        {
            Window = window;
            CandlesViewed = candles_viewed;
            Indent = indent;
            NotTradingTime = not_trading_time;
            Tool = tool;

            InitialIndicators();
        }

        public void InitialIndicators()
        {
            Extremums = new List<(Candle, Extremum)>();
        }

        public void FindExtremums()
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
