using StopOrder = QuikSharp.DataStructures.StopOrder;
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

        public abstract void AnalysisCandle(Candle candle);
        public abstract void AnalysisTick(Tick tick);

        /// <summary>
        /// Отправка ордеров.
        /// </summary>
        /// <param name="last_extremum"></param>
        /// <param name="price"></param>
        /// <param name="operation"></param>
        public abstract void PlacingOrders((Candle, Extremum) last_extremum, decimal price, Operation operation, DateTime dateTime);

        /// <summary>
        /// Отправка стоп-лимитов.
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public abstract List<OrderInfo> PlacingStopLimitOrder(Deal deal);

        /// <summary>
        /// Отправка тейк-профитов.
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public abstract List<OrderInfo> PlacingTakeProfitOrder(Deal deal);

        /// <summary>
        /// Пересчет стоп-лимитов.
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public abstract OrderInfo RecalculateStopLimit(Deal deal);

        /// <summary>
        /// Пересчет тейк-профитов.
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public abstract OrderInfo RecalculateTakeProfit(Deal deal);

        public abstract void ProcessingExecutedOrders(Order order);
        public abstract void ProcessingExecutedStopOrders(StopOrder stoporder);
    }
}
