using QuikSharp.DataStructures;
using System;

namespace TradingRobotsServer.Models.Structures
{
    public class StockData
    {
        public virtual int ID { get; set; }
        public DateTime DateTime { get; set; }
        public int Vol { get; set; }

        #region Candles subscription info

        /// <summary>
        /// Код инструмента.
        /// </summary>
        public string SecCode { get; set; }

        /// <summary>
        /// Код класса.
        /// </summary>
        public string ClassCode { get; set; }

        /// <summary>
        /// Интервал подписки.
        /// </summary>
        public CandleInterval Interval { get; set; }

        #endregion
    }
}
