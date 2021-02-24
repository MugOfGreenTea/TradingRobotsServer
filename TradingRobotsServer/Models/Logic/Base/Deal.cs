using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System.Collections.Generic;

namespace TradingRobotsServer.Models.Structures
{
    public class Deal
    {
        public int ID { get; set; }
        public TrendDataPoint TradeEntryPoint { get; set; }
        public decimal StopLoss { get; set; }
        public TrendDataPoint ExitPoint { get; set; }
        public TrendDataPoint SignalPoint { get; set; }
        public StatusDeal Status { get; set; }
        public Operation Operation { get; set; }
        public ReasonStop ReasonStop { get; set; }
        public int Vol { get; set; }
        public int PerVol { get; set; }
        public int LastVol { get; set; }

        public List<string> Logs;

        public Deal()
        {
            Logs = new List<string>();
        }

        public Deal(TrendDataPoint trade_entry_point, TrendDataPoint signal_point, StatusDeal status, Operation operation, int vol)
        {
            TradeEntryPoint = new TrendDataPoint(trade_entry_point);
            SignalPoint = new TrendDataPoint(signal_point);
            Status = status;
            Operation = operation;
            Vol += vol;
            PerVol += vol;
        }

        public void LogDeal(string log_str)
        {
            Logs.Add(Logs.Count + ":" + log_str + ";\r\n");
        }
    }
}
