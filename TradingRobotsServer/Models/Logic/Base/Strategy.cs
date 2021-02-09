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
        public delegate void OnNewOrder(Deal deal);
        public abstract event OnNewOrder NewOrder;

        //public delegate void OnNewStopOrder(List<OrderInfo> new_stop_order);
        //public abstract event OnNewStopOrder NewStopOrder;

        public abstract void AnalysisCandle(Candle candle);
        public abstract void AnalysisTick(Tick tick);
        public abstract List<OrderInfo> PlacingStopOrder(decimal price, Operation operation);
    }
}
