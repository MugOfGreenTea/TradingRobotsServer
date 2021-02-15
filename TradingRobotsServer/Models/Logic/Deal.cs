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
        public int Vol { get; set; }
        public int PerVol { get; set; }
        public int LastVol { get; set; }

        public List<OrderInfo> OrdersInfo { get; set; }
        public List<OrderInfo> StopLimitOrdersInfo { get; set; }
        public List<OrderInfo> TakeProfitOrdersInfo { get; set; }
        public List<OrderInfo> TakeProfitAndStopLimitOrdersInfo { get; set; }

        //public List<Order> Orders { get; set; }
        //public List<(StopOrder, Order)> StopLimitOrders { get; set; }
        //public List<(StopOrder, Order)> TakeProfitOrders { get; set; }

        public Deal()
        {
            OrdersInfo = new List<OrderInfo>();
            StopLimitOrdersInfo = new List<OrderInfo>();
            TakeProfitOrdersInfo = new List<OrderInfo>();
            TakeProfitAndStopLimitOrdersInfo = new List<OrderInfo>();

            //Orders = new List<Order>();
            //StopLimitOrders = new List<(StopOrder, Order)>();
            //TakeProfitOrders = new List<(StopOrder, Order)>();
        }

        public Deal(TrendDataPoint trade_entry_point, TrendDataPoint signal_point, StatusDeal status, Operation operation, int vol)
        {
            TradeEntryPoint = new TrendDataPoint(trade_entry_point);
            SignalPoint = new TrendDataPoint(signal_point);
            Status = status;
            Operation = operation;
            Vol += vol;
            PerVol += vol;

            OrdersInfo = new List<OrderInfo>();
            StopLimitOrdersInfo = new List<OrderInfo>();
            TakeProfitOrdersInfo = new List<OrderInfo>();
            TakeProfitAndStopLimitOrdersInfo = new List<OrderInfo>();

            //Orders = new List<Order>();
            //StopLimitOrders = new List<(StopOrder, Order)>();
            //TakeProfitOrders = new List<(StopOrder, Order)>();
        }
    }
}
