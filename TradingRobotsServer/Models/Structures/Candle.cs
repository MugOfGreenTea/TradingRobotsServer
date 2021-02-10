using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public class Candle : QuikSharp.DataStructures.Candle
    {
        public virtual int ID { get; set; }
        public DateTime DateTime { get; set; }
        public int Vol { get; set; }
        public TypeCandle TypeCandle { get; set; }

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

        public Candle(QuikSharp.DataStructures.Candle candle)
        {
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

        public override string ToString()
        {
            return $"SecCode: {SecCode}, ID: {ID}, DateTime: {DateTime}, Open: {Open}, Close: {Close}, High: {High}, Low: {Low}, Volume: {Vol}";
        }
    }
}
