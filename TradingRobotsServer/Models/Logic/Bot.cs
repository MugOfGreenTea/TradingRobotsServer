using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Operation = QuikSharp.DataStructures.Operation;
using TradingRobotsServer.Models.Logic.Base;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Logic
{
    public class Bot
    {
        QuikConnect QuikConnecting;
        Tool Tool;
        Strategy Strategy;

        public List<Deal> Deals;

        public Bot(QuikConnect quikConnect, Tool tool, Strategy strategy)
        {
            QuikConnecting = quikConnect;
            Tool = tool;
            Strategy = strategy;
            Deals = new List<Deal>();
        }

        /// <summary>
        /// Подписка на данные от инструмента.
        /// </summary>
        public void SubsribeNewDataTool()
        {
            Tool.NewCandle += NewCandle;
            Tool.GetHistoricalCandlesAsync(15);
            Tool.NewTick += NewTick;
        }

        /// <summary>
        /// Подписка на ордера от стратегии.
        /// </summary>
        public void SubsribeNewOrderStrategy()
        {
            Strategy.NewOrder += SendingOrder;
        }
        
        /// <summary>
        /// Обработчик события новой свечи.
        /// </summary>
        /// <param name="candle"></param>
        public void NewCandle(Candle candle)
        {
            Strategy.AnalysisCandle(candle);
        }

        /// <summary>
        /// Обработчик события нового тика.
        /// </summary>
        /// <param name="tick"></param>
        public void NewTick(Tick tick)
        {
            Strategy.AnalysisTick(tick);
        }

        /// <summary>
        /// Обработчик события нового ордера.
        /// </summary>
        /// <param name="order"></param>
        public void SendingOrder(Deal deal)
        {
            Debug.WriteLine("Получен ордер в Bot");

            if (!CheckDeal(deal))
                return;
            deal.Status = StatusDeal.Open;
            Deals.Add(deal);

            foreach (OrderInfo order in deal.Orders)
            {
                switch (order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.LimitOrder:
                        QuikConnecting.LimitOrder(Tool, order.Operation, order.Price, order.Vol);
                        break;
                    case TypeOrder.MarketOrder:
                        QuikConnecting.MarketOrder(Tool, order.Operation, order.Vol);
                        break;
                    case TypeOrder.TakeProfit:
                        QuikConnecting.TakeProfitOrder(Tool, 0, 0, order.Price, order.Price, order.Operation, order.Vol);
                        break;
                    case TypeOrder.StopLimit:
                        QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, order.Price, order.Price, order.Operation, order.Vol);
                        break;
                    case TypeOrder.TakeProfitAndStopLimit:
                        break;
                    default:
                        break;
                }
            }
        }

        private bool check_closed_deal = true;
        public bool CheckDeal(Deal deal)
        {
            check_closed_deal = true;

            for (int i = 0; i < Deals.Count; i++)
            {
                if ((Deals[i].Status == StatusDeal.Open || Deals[i].Status == StatusDeal.Closed) && Deals[i].Operation == deal.Operation && deal.SignalPoint.XPoint == Deals[i].SignalPoint.XPoint)
                {
                    check_closed_deal = false;
                }
            }

            return check_closed_deal;
        }
    }
}
