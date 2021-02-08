using QuikSharp.DataStructures;
using System;

namespace TradingRobotsServer.Models.Structures
{
    public class Tick : StockData
    {
        public decimal Price { get; set; }

        public Tick(string sec_code, string class_code, CandleInterval interval, decimal price)
        {
            SecCode = sec_code;
            ClassCode = class_code;
            Interval = interval;
            Price = price;
        }

        public Tick(Tick tick)
        {
            SecCode = tick.SecCode;
            ClassCode = tick.ClassCode;
            Interval = tick.Interval;
            DateTime = tick.DateTime;
            Price = tick.Price;
            Vol = tick.Vol;
        }
    }
}
