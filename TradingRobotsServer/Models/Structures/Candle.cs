using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public class Candle : StockData
    {
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public TypeCandle TypeCandle { get; set; }

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

        public Candle(int id, string sec_code, string class_code, CandleInterval interval, DateTime dateTime,
                decimal open, decimal high, decimal low, decimal close, int vol)
        {
            ID = id;
            SecCode = sec_code;
            ClassCode = class_code;
            Interval = interval;
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Vol = vol;

            if (Open < Close)
                TypeCandle = TypeCandle.Growing;
            else
                TypeCandle = TypeCandle.Falling;
        }

        public Candle(int id, QuikSharp.DataStructures.Candle candle)
        {
            ID = id;
            SecCode = candle.SecCode;
            ClassCode = candle.ClassCode;
            Interval = candle.Interval;
            DateTime = (DateTime)candle.Datetime;
            Open = candle.Open;
            High = candle.High;
            Low = candle.Low;
            Close = candle.Close;
            Vol = candle.Volume;

            if (Open < Close)
                TypeCandle = TypeCandle.Growing;
            else
                TypeCandle = TypeCandle.Falling;
        }
    }
}
