using QuikSharp.DataStructures;
using System.Collections.Generic;

namespace TradingRobotsServer.Models.Structures
{
    public class Deal
    {
        public TrendDataPoint TradeEntryPoint { get; set; }
        public TrendDataPoint ExitPoint { get; set; }
        public TrendDataPoint SignalPoint { get; set; }
        public StatusDeal Status { get; set; }
        public Operation Operation { get; set; }
        public int Vol { get; set; }
        public int PerVol { get; set; }
        public int LastVol { get; set; }

        public List<OrderInfo> Orders { get; set; }

        public Deal()
        {
            Orders = new List<OrderInfo>();
        }

        public Deal(TrendDataPoint trade_entry_point, TrendDataPoint signal_point, StatusDeal status, Operation operation, int vol)
        {
            TradeEntryPoint = new TrendDataPoint(trade_entry_point);
            SignalPoint = new TrendDataPoint(signal_point);
            Status = status;
            Operation = operation;
            Vol += vol;
            PerVol += vol;

            Orders = new List<OrderInfo>();
        }
    }
}
