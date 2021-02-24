using QuikSharp.DataStructures;
using System.Collections.Generic;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Logic
{
    public class DealHighLow : Deal
    {
        public decimal Distance { get; set; }

        public OrderInfo EntryOrder { get; set; }
        public OrderInfo FirstTakeProfitOrder { get; set; }
        public OrderInfo FirstStopLossOrder { get; set; }
        public OrderInfo SecondTakeProfitOrder { get; set; }
        public OrderInfo SecondStopLossOrder { get; set; }
        public OrderInfo ThirdTakeProfitOrder { get; set; }
        public OrderInfo ThirdStopLossOrder { get; set; }

        //public List<(OrderInfo, TakeProfitsAndStopLoss)> Orders { get; set; } = new List<(OrderInfo, TakeProfitsAndStopLoss)>();
    }

}
