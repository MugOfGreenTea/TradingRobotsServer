using QuikSharp.DataStructures.Transaction;
using State = QuikSharp.DataStructures.State;
using System.Collections.Generic;
using System.Diagnostics;
using TradingRobotsServer.Models.Logic.Base;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Structures;
using System.Threading;
using StopOrder = QuikSharp.DataStructures.StopOrder;
using System;
using System.Linq;

namespace TradingRobotsServer.Models.Logic
{
    public class OldBot
    {
        private QuikConnect QuikConnecting;
        private Tool Tool;
        private Strategy Strategy;

        public List<Deal> Deals;

        public OldBot(QuikConnect quikConnect, Tool tool, Strategy strategy)
        {
            QuikConnecting = quikConnect;
            Tool = tool;
            Strategy = strategy;
            Deals = new List<Deal>();
        }

        #region Подписки

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
        /// Подписка на получение информации о новой заявке или изменения выставленной заявки.
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
        /// Подписка на получение изменений позиции в стоп-заявках.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeOnStopOrder()
        {
            try
            {
                DebugLog("Подписываемся на изменение позиции в стоп-заявках...");
                QuikConnecting.quik.Events.OnStopOrder += OnStopOrderDo;
                DebugLog("Подписка включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции в стоп-заявках не удалась.");
                return false;
            }
        }

        #endregion

        #region Обработчики событий

        #region Обработка новых данных

        /// <summary>
        /// Обработчик события новой свечи.
        /// </summary>
        /// <param name="candle"></param>
        private void NewCandle(Candle candle)
        {
            Strategy.AnalysisCandle(candle);
        }

        /// <summary>
        /// Обработчик события нового тика.
        /// </summary>
        /// <param name="tick"></param>
        private void NewTick(Tick tick)
        {
            Strategy.AnalysisTick(tick);
        }

        #endregion

        #region Обработка событий от Quik

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
        /// Обработчик события получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <param name="tick"></param>
        private void OnOrder(Order order)
        {
            Thread.Sleep(200);
            for (int i = 0; i < Deals.Count; i++)
            {
                if (Deals[i].Orders.Count == 0)
                    continue;                
                for (int j = 0; j < Deals[i].Orders.Count; j++)
                {
                    if (order.OrderNum == Deals[i].Orders[j].OrderNum)
                    {
                        Deals[i].Orders[j] = order;

                        if (Deals[i].Orders[j].State == State.Completed)
                        {
                            SendStopLimitOrders(i);
                            SendTakProfitOrders(i);
                        }
                    }
                }
            }

            for (int i = 0; i < Deals.Count; i++)
            {
                if (Deals[i].StopLimitOrders.Count == 0)
                    continue;
                for (int j = 0; j < Deals[i].StopLimitOrders.Count; j++)
                {
                    if (order.OrderNum == Deals[i].StopLimitOrders[j].Item2.OrderNum && Deals[i].StopLimitOrders[j].Item1.StopOrderType == QuikSharp.DataStructures.StopOrderType.TakeProfit)
                    {
                        Deals[i].StopLimitOrders[j] = (Deals[i].StopLimitOrders[j].Item1, order);

                        if (Deals[i].StopLimitOrders[j].Item2.State == State.Completed)
                        {
                            
                        }
                    }

                    if (order.OrderNum == Deals[i].StopLimitOrders[j].Item2.OrderNum && Deals[i].StopLimitOrders[j].Item1.StopOrderType == QuikSharp.DataStructures.StopOrderType.StopLimit)
                    {
                        Deals[i].StopLimitOrders[j] = (Deals[i].StopLimitOrders[j].Item1, order);

                        if (Deals[i].StopLimitOrders[j].Item2.State == State.Completed)
                        {
                            Deals[i].Status = StatusDeal.Closed;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private void OnStopOrderDo(StopOrder stoporder)
        {
            DebugLog("Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State);
            DebugLog("Вызвано событие OnStopOrder - связ. заявка: " + stoporder.LinkedOrder);

            for (int i = 0; i < Deals.Count; i++)
            {
                if (Deals[i].Orders.Count == 0)
                    continue;
                for (int j = 0; j < Deals[i].Orders.Count; j++)
                {
                    if (stoporder.TransId == Deals[i].StopLimitOrders[j].Item1.TransId)
                    {
                        Deals[i].StopLimitOrders[j] = (stoporder, null);

                        if (Deals[i].StopLimitOrders[j].Item1.State == State.Completed)
                        {
                            Deals[i].StopLimitOrders[j] = (stoporder, FindOrderFromStopOrder(stoporder.OrderNum));
                        }
                    }
                }
            }
        }

        private Order FindOrderFromStopOrder(long order_num)
        {
            List<Order> listOrders = QuikConnecting.GetOrdersTable();
            foreach (Order order in listOrders)
            {
                if(order.OrderNum == order_num)
                    return order;
            }

            return null;
        }

        #endregion

        #region Обработка сигналов от стратегии

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
        private bool CheckDeal(Deal deal)
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

        #endregion

        #endregion

        /// <summary>
        /// Выставление ордеров.
        /// </summary>
        /// <param name="deal"></param>
        public void SendOrders(ref Deal deal)
        {
            foreach (OrderInfo order in deal.OrdersInfo)
            {
                if (order.State == State.Completed)
                    continue;
                switch (order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.LimitOrder:
                        deal.Orders.Add(QuikConnecting.LimitOrder(Tool, order.Operation, order.Price, order.Vol).Result);
                        deal.StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.MarketOrder:
                        deal.Orders.Add(QuikConnecting.MarketOrder(Tool, order.Operation, order.Vol).Result);
                        deal.StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Выставление стоп-ордеров.
        /// </summary>
        /// <param name="deal"></param>
        public void SendStopLimitOrders(int i)
        {
            Deals[i].StopLimitOrdersInfo.AddRange(Strategy.PlacingStopLimitOrder(Deals[i]));

            foreach (OrderInfo stop_order in Deals[i].StopLimitOrdersInfo)
            {
                if (stop_order.State == State.Completed)
                    continue;
                switch (stop_order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.TakeProfitOrder(Tool, 0, 0, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.StopLimit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.TakeProfitAndStopLimit:

                        break;
                    default:
                        break;
                }
            }
        }

        public void SendTakProfitOrders(int i)
        {
            Deals[i].StopLimitOrdersInfo.AddRange(Strategy.PlacingTakeProfitOrder(Deals[i]));

            foreach (OrderInfo stop_order in Deals[i].StopLimitOrdersInfo)
            {
                if (stop_order.State == State.Completed)
                    continue;
                switch (stop_order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.TakeProfitOrder(Tool, 0, 0, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.StopLimit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.TakeProfitAndStopLimit:

                        break;
                    default:
                        break;
                }
            }
        }

        public void SendStopLoss(int i)
        {
            Deals[i].StopLimitOrdersInfo.Add(Strategy.RecalculateStopLimit(Deals[i]));

            foreach (OrderInfo stop_order in Deals[i].StopLimitOrdersInfo)
            {
                if (stop_order.State == State.Completed)
                    continue;
                switch (stop_order.TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.TakeProfitOrder(Tool, 0, 0, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
                        break;
                    case TypeOrder.StopLimit:
                        Deals[i].StopLimitOrders.Add((QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, stop_order.Price, stop_order.Price, stop_order.Operation, stop_order.Vol).Result, null));
                        Deals[i].StopLimitOrdersInfo.Last().State = State.Completed;
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
