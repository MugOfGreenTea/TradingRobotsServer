using QuikSharp.DataStructures.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;
using Operation = QuikSharp.DataStructures.Operation;

namespace TradingRobotsServer.Models.Logic.Base
{
    public abstract class Strategy
    {
        public abstract Bot Bot { get; set; }

        public delegate void OnNewOrder(Deal deal, Command command);
        public abstract event OnNewOrder NewOrder;

        public abstract void SubsribeNewDeal();

        //public delegate void OnNewStopOrder(List<OrderInfo> new_stop_order);
        //public abstract event OnNewStopOrder NewStopOrder;

        public abstract void AnalysisCandle(Candle candle);
        public abstract void AnalysisTick(Tick tick);
        public abstract void PlacingOrders((Candle, Extremum) last_extremum, decimal price, Operation operation);
        public abstract List<OrderInfo> PlacingStopLimitOrder(Deal deal);
        public abstract List<OrderInfo> PlacingTakeProfitOrder(Deal deal);
        public abstract OrderInfo RecalculateStopLimit(Deal deal);
        public abstract OrderInfo RecalculateTakeProfit(Deal deal);
        public abstract void ProcessingExecutedOrders(Order order);

    }
}
