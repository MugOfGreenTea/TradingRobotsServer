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
using TradingRobotsServer.Models.Support;
using NLog;

namespace TradingRobotsServer.Models.Logic
{
    public class Bot
    {
        private QuikConnect QuikConnecting;
        private Tool Tool;
        private Strategy Strategy;

        public delegate void OnNewDeal(Deal deal, Command command);
        public event OnNewDeal NewDeal;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private int recursion_counter = 5;

        public Bot(QuikConnect quikConnect, Tool tool, Strategy strategy)
        {
            QuikConnecting = quikConnect;
            Tool = tool;
            Strategy = strategy;
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
                QuikConnecting.quik.Events.OnStopOrder += OnStopOrder;
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

        }

        private long temp_id_order = -1;
        /// <summary>
        /// Обработчик события получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <param name="tick"></param>
        private void OnOrder(Order order)
        {
            if (order.State == State.Completed && order.OrderNum != temp_id_order)
            {
                temp_id_order = order.OrderNum;
                Strategy.ProcessingExecutedOrders(order);
            }
        }

        private long temp_id_stoporder = -1;
        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private void OnStopOrder(StopOrder stoporder)
        {
            DebugLog("Вызвано событие OnStopOrder - 'Time' = " + DateTime.Now + ", 'OrderNum' = " + stoporder.OrderNum + ", 'State' = " + stoporder.State);
            DebugLog("Вызвано событие OnStopOrder - связ. заявка: " + stoporder.LinkedOrder);

            if (stoporder.State == State.Completed && stoporder.OrderNum != temp_id_stoporder)
                Strategy.ProcessingExecutedStopOrders(stoporder);
        }

        private Order FindOrderFromStopOrder(long order_num)
        {
            List<Order> listOrders = QuikConnecting.GetOrdersTable();
            foreach (Order order in listOrders)
            {
                if (order.OrderNum == order_num)
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
        public void NewOrder(Deal deal, Command command)
        {
            DebugLog("Ордер получен в Bot");

            switch (command)
            {
                case Command.Null:
                    break;
                case Command.SendOrder:
                    SendOrders(ref deal);
                    break;
                case Command.SendStopLimitOrder:
                    SendStopLimitOrders(ref deal);
                    break;
                case Command.SendTakeProfitOrder:
                    SendTakeProfitOrders(ref deal);
                    break;
                case Command.SendTakeProfitAndStopLimitOrder:
                    SendtakeProfitAndStopLimitOrders(ref deal);
                    break;
                case Command.TakeOffOrder:
                    break;
                case Command.TakeOffStopLimitOrder:
                    TakeOffStopLimitOrder(ref deal);
                    break;
                case Command.TakeOffTakeProfitOrder:
                    TakeOffTakeProfitOrder(ref deal);
                    break;
                default:
                    break;
            }

            NewDeal?.Invoke(deal, command);
        }

        #region Методы отправки и снятия ордеров

        /// <summary>
        /// Выставление ордеров.
        /// </summary>
        /// <param name="deal"></param>
        private void SendOrders(ref Deal deal)
        {
            for (int i = 0; i < deal.OrdersInfo.Count; i++)
            {
                if (deal.OrdersInfo[i].IssueStatus == State.Completed || deal.OrdersInfo[i].IssueStatus == State.Canceled)
                    continue;

                Order temp_order;
                switch (deal.OrdersInfo[i].TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.LimitOrder:
                        deal.OrdersInfo[i].Vol = ManagementOfRisks.CalculationCountLotsToTradeFutures(QuikConnecting, Tool, deal, deal.StopLoss, 1, Tool.Lot);
                        temp_order = QuikConnecting.LimitOrder(Tool, deal.OrdersInfo[i].Operation, deal.OrdersInfo[i].Price, deal.OrdersInfo[i].Vol).Result;
                        if (temp_order != null)
                        {
                            deal.OrdersInfo[i].IDOrder = temp_order.OrderNum;
                            deal.OrdersInfo[i].IssueStatus = State.Completed;
                            deal.Status = StatusDeal.Open;
                        }
                        else
                        {
                            //впилить защиту от не открытой сделки
                        }
                        break;
                    case TypeOrder.MarketOrder:
                        temp_order = QuikConnecting.MarketOrder(Tool, deal.OrdersInfo[i].Operation, deal.OrdersInfo[i].Vol).Result;
                        if (temp_order != null)
                        {
                            deal.OrdersInfo[i].IDOrder = temp_order.OrderNum;
                            deal.OrdersInfo[i].IssueStatus = State.Completed;
                        }
                        else
                        {
                            //впилить защиту от не открытой сделки
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SendStopLimitOrders(ref Deal deal)
        {
            if (deal.Status != StatusDeal.Open)
                return;

            for (int i = 0; i < deal.StopLimitOrdersInfo.Count; i++)
            {
                if (deal.StopLimitOrdersInfo[i].IssueStatus == State.Completed || deal.StopLimitOrdersInfo[i].IssueStatus == State.Canceled)
                    continue;
                StopOrder temp_stoporder;
                switch (deal.StopLimitOrdersInfo[i].TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.StopLimit:
                        temp_stoporder = QuikConnecting.StopLimitOrder(Tool, 0.5m, 0.1m, deal.StopLimitOrdersInfo[i].Price, deal.StopLimitOrdersInfo[i].Price, deal.StopLimitOrdersInfo[i].Operation, deal.Vol).Result;
                        if (temp_stoporder != null)
                        {
                            deal.StopLimitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                            deal.StopLimitOrdersInfo[i].IssueStatus = State.Completed;
                        }
                        else
                        {
                            //впилить защиту от не открытой сделки
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SendTakeProfitOrders(ref Deal deal)
        {
            if (deal.Status != StatusDeal.Open)
                return;

            for (int i = 0; i < deal.TakeProfitOrdersInfo.Count; i++)
            {
                if (deal.TakeProfitOrdersInfo[i].IssueStatus == State.Completed || deal.TakeProfitOrdersInfo[i].IssueStatus == State.Canceled)
                    continue;
                StopOrder temp_stoporder;
                switch (deal.TakeProfitOrdersInfo[i].TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfit:
                        deal.OrdersInfo[i].Vol = ManagementOfRisks.CalculationCountPartSale(deal, 3, Tool.Lot);
                        temp_stoporder = QuikConnecting.TakeProfitOrder(Tool, 0, 0, deal.TakeProfitOrdersInfo[i].Price, deal.TakeProfitOrdersInfo[i].Price,
                            deal.TakeProfitOrdersInfo[i].Operation, deal.TakeProfitOrdersInfo[i].Vol).Result;
                        if (temp_stoporder != null)
                        {
                            deal.TakeProfitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                            deal.TakeProfitOrdersInfo[i].IssueStatus = State.Completed;
                        }
                        else
                        {
                            //впилить защиту от не открытой сделки
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SendtakeProfitAndStopLimitOrders(ref Deal deal)
        {
            if (deal.Status != StatusDeal.Open)
                return;

            for (int i = 0; i < deal.TakeProfitAndStopLimitOrdersInfo.Count; i++)
            {
                if (deal.TakeProfitAndStopLimitOrdersInfo[i].IssueStatus == State.Completed || deal.TakeProfitAndStopLimitOrdersInfo[i].IssueStatus == State.Canceled)
                    continue;
                StopOrder temp_stoporder;
                switch (deal.TakeProfitAndStopLimitOrdersInfo[i].TypeOrder)
                {
                    case TypeOrder.Null:
                        break;
                    case TypeOrder.TakeProfitAndStopLimit:
                        temp_stoporder = QuikConnecting.TakeProfitStotLimitOrder(Tool, 0, 0, deal.TakeProfitAndStopLimitOrdersInfo[i].Price, deal.TakeProfitAndStopLimitOrdersInfo[i].Price2,
                            deal.TakeProfitAndStopLimitOrdersInfo[i].Price3, deal.TakeProfitAndStopLimitOrdersInfo[i].Operation, deal.TakeProfitAndStopLimitOrdersInfo[i].Vol).Result;
                        if (temp_stoporder != null)
                        {
                            deal.TakeProfitAndStopLimitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                            deal.TakeProfitAndStopLimitOrdersInfo[i].IssueStatus = State.Completed;
                        }
                        else
                        {
                            //впилить защиту от не открытой сделки
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void TakeOffStopLimitOrder(ref Deal deal)
        {
            for (int i = 0; i < deal.StopLimitOrdersInfo.Count; i++)
            {
                if (deal.StopLimitOrdersInfo[i].ExecutionStatus == State.Canceled)
                {
                    QuikConnecting.TakeOffStopOrder(deal.StopLimitOrdersInfo[i].IDOrder);
                }
            }
        }

        private void TakeOffTakeProfitOrder(ref Deal deal)
        {
            for (int i = 0; i < deal.TakeProfitOrdersInfo.Count; i++)
            {
                if (deal.TakeProfitOrdersInfo[i].ExecutionStatus == State.Canceled)
                {
                    QuikConnecting.TakeOffStopOrder(deal.TakeProfitOrdersInfo[i].IDOrder);
                }
            }
        }

        #endregion Методы отправки и снятия ордеров

        #endregion Обработка сигналов от стратегии

        #endregion Обработчики событий

        #region Лог

        public void DebugLog(string log_string)
        {
            Debug.WriteLine(log_string);
            logger.Info(log_string);
        }

        #endregion
    }
}
