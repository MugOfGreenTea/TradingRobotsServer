using QuikSharp.DataStructures.Transaction;
using State = QuikSharp.DataStructures.State;
using System.Collections.Generic;
using System.Diagnostics;
using TradingRobotsServer.Models.Logic.Base;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Logic
{
    public class Bot
    {
        private QuikConnect QuikConnecting;
        private Tool Tool;
        private Strategy Strategy;

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
            DebugLog("Подписка на новые свечи от Tool");
            Tool.GetHistoricalCandlesAsync(20);
            DebugLog("Получение исторических свеч от Tool");
            Tool.NewTick += NewTick;
            DebugLog("Подписка на новые тики от Tool");
        }

        /// <summary>
        /// Подписка на ордера от стратегии.
        /// </summary>
        public void SubsribeNewOrderStrategy()
        {
            Strategy.NewOrder += NewOrder;
            DebugLog("Подписка на новые ордера от Strategy");
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
        /// Подписка на получение информации о новой сделке.
        /// </summary>
        /// <returns></returns>
        public bool SubsribeOnTrade()
        {
            try
            {
                DebugLog("Подписываемся на получение информации о новой сделке...");
                QuikConnecting.quik.Events.OnTrade += OnTrade;
                DebugLog("Подписка включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на получение информации о новой сделке не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Обработчик события получение информации о новой сделке.
        /// </summary>
        /// <param name="tick"></param>
        private void OnTrade(Trade trade)
        {
            DebugLog("Произошло OnTrade.");
            DebugLog("OrderNum - номер заявки: " + trade.OrderNum);
            DebugLog("TradeNum - номер сделки." + trade.TradeNum);
            DebugLog("price: " + trade.Price);
            DebugLog("vol: " + trade.Quantity);
            DebugLog("SettleCode - код расчетов: " + trade.SettleCode);
            DebugLog("SecCode: " + trade.SecCode);
            DebugLog("TransID: " + trade.TransID);
        }

        /// <summary>
        /// Подписка на получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <returns></returns>
        public bool SubsribeOnOrder()
        {
            try
            {
                DebugLog("Подписываемся на получение информации о новой заявке или изменения выставенной заявки...");
                QuikConnecting.quik.Events.OnOrder += OnOrder;
                DebugLog("Подписка включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на получение информации о новой заявке или изменения выставенной заявки не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Обработчик события получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <param name="tick"></param>
        private void OnOrder(Order order)
        { 
            for (int i = 0; i < Deals.Count; i++)
            {
                for (int j = 0; j < Deals[i].Orders.Count; j++)
                {
                    if (order.OrderNum == Deals[i].Orders[j].OrderNum)
                    {
                        Deals[i].Orders[j] = order;

                        if (Deals[i].Orders[j].State == State.Completed)
                        {
                            SendStopOrders(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события нового ордера.
        /// </summary>
        /// <param name="order"></param>
        public void NewOrder(Deal deal)
        {
            Debug.WriteLine("Ордер получен в Bot");

            if (!CheckDeal(deal))
                return;

            deal.Status = StatusDeal.Open;

            SendOrders(ref deal);

            Deals.Add(deal);
            Debug.WriteLine("Ордер добавлен в список");
        }

        private bool check_closed_deal = true;
        /// <summary>
        /// Проверка на повторяющиеся сделки.
        /// </summary>
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

        /// <summary>
        /// Исполнение ордеров.
        /// </summary>
        /// <param name="deal"></param>
        public void SendOrders(ref Deal deal)
        {
            foreach (OrderInfo order in deal.OrdersInfo)
            {
                switch (order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.LimitOrder:
                        deal.Orders.Add(QuikConnecting.LimitOrder(Tool, order.Operation, order.Price, order.Vol).Result);
                        break;
                    case TypeOrder.MarketOrder:
                        deal.Orders.Add(QuikConnecting.MarketOrder(Tool, order.Operation, order.Vol).Result);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Исполнение стоп-ордеров.
        /// </summary>
        /// <param name="deal"></param>
        public void SendStopOrders(int i)
        {
            Deals[i].StopOrdersInfo.AddRange(Strategy.PlacingStopOrder(Deals[i].Orders[0].Price, Deals[i].Operation));

            foreach (OrderInfo stop_order in Deals[i].StopOrdersInfo)
            {
                switch (stop_order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfit:
                        Deals[i].StopOrders.Add(QuikConnecting.TakeProfitOrder(Tool, 0, 0, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result);
                        break;
                    case TypeOrder.StopLimit:
                        Deals[i].StopOrders.Add(QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result);
                        break;
                    case TypeOrder.TakeProfitAndStopLimit:

                        break;
                    default:
                        break;
                }
            }
        }

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
        }

        #endregion
    }
}
