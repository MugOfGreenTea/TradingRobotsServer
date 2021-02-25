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
using System.Threading.Tasks;
using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Logic
{
    public class Bot
    {
        private QuikConnect QuikConnecting;
        public Tool Tool;
        private Strategy Strategy;

        public delegate void OnNewDeal(OrderInfo order);
        public event OnNewDeal NewDeal;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private int recursion_counter = 5;

        public List<OrderInPool> PoolOrders;

        public Bot(QuikConnect quikConnect, Tool tool, Strategy strategy)
        {
            QuikConnecting = quikConnect;
            Tool = tool;
            Strategy = strategy;

            PoolOrders = new List<OrderInPool>();
        }

        #region Подписки

        /// <summary>
        /// Подписка на новые свечи от инструмента.
        /// </summary>
        public void SubsribeNewCandleToolAsync()
        {
            Tool.NewCandle += NewCandle;
            //Tool.GetHistoricalCandlesAsync(10);
            DebugLog("Bot: Подписка на новые свечки от Tool завершена.");
        }

        /// <summary>
        /// Подписка на новые тики от инструмента.
        /// </summary>
        public void SubsribeNewTickTool()
        {
            Tool.NewTick += NewTick;
            DebugLog("Bot: Подписка на новые тики от Tool завершена.");
        }

        /// <summary>
        /// Подписка на ордера от стратегии.
        /// </summary>
        public void SubsribeNewOrderStrategy()
        {
            Strategy.NewOrder += NewOrder;
            DebugLog("Bot: Подписка на новые ордера от Strategy завершена.");
        }

        /// <summary>
        /// Подписка на получение информации о новой сделке.
        /// </summary>
        /// <returns></returns>
        public bool SubsribeOnTrade()
        {
            try
            {
                QuikConnecting.quik.Events.OnTrade += OnTrade;
                DebugLog("Подписка на получение информации о новой сделке включена...");
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
                QuikConnecting.quik.Events.OnOrder += OnOrder;
                DebugLog("Подписка на получение информации о новой заявке или изменения выставенной заявки включена...");
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
                QuikConnecting.quik.Events.OnStopOrder += OnStopOrder;
                DebugLog("Подписка на изменение позиции в стоп-заявках включена...");
                return true;
            }
            catch
            {
                DebugLog("Подписка на изменение позиции в стоп-заявках не удалась.");
                return false;
            }
        }

        /// <summary>
        /// Подписка на получение изменений в таблице текущий торгов.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeOnParam()
        {
            try
            {
                QuikConnecting.quik.Events.OnParam += OnParam;
                Logs.DebugLog("Подписка на получение информации о новой сделке включена...", LogType.Info);
                return true;
            }
            catch
            {
                Logs.DebugLog("Подписка на получение информации о новой сделке не удалась.", LogType.Error);
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
        private void NewCandle(Structures.Candle candle)
        {
            Strategy.AnalysisCandle(candle);
            if (PoolOrders.Count != 0)
                DistributionOrders();
        }

        /// <summary>
        /// Обработчик события нового тика.
        /// </summary>
        /// <param name="tick"></param>
        private void NewTick(Tick tick)
        {
            Strategy.AnalysisTick(tick);
            if (PoolOrders.Count != 0)
                DistributionOrders();
        }

        #endregion

        #region Обработка событий от Quik

        private long temp_id_order = -1;
        /// <summary>
        /// Обработчик события получение информации о новой сделке.
        /// </summary>
        /// <param name="tick"></param>
        private void OnTrade(Trade trade)
        {
            Debug.WriteLine("! Comment: " + trade.Comment + ", OrderNum: " + trade.OrderNum);
            if (trade.Account == Tool.AccountID && trade.SecCode == Tool.SecurityCode && temp_id_order != trade.OrderNum)
            {
                temp_id_order = trade.OrderNum;

                bool check = Strategy.ProcessingExecutedOrders(trade);
                if (!check)
                {
                    PoolOrders.Add(new OrderInPool(trade));
                    Debug.WriteLine("OnOrder попал в Pool");
                }
            }
        }

        /// <summary>
        /// Обработчик события изменения Таблицы текущих торгов.
        /// </summary>
        /// <param name="param"></param>
        private void OnParam(Param param)
        {
            if (param.SecCode == Tool.SecurityCode)
            {
                decimal status_cliring = Convert.ToDecimal(QuikConnecting.GetParamEx(param, ParamNames.CLSTATE));
                if (Tool.StatusClearing != (StatusClearing)status_cliring)
                    Tool.UpdateParam();
            }
        }

        /// <summary>
        /// Обработчик события получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <param name="tick"></param>
        private async void OnOrder(Order order)
        {

        }

        private long temp_id_stoporder = -1;
        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private async void OnStopOrder(StopOrder stoporder)
        {
            Debug.WriteLine("! Comment: " + stoporder.Comment + ", OrderNum: " + stoporder.OrderNum + ", LinkedOrder: " + stoporder.LinkedOrder + ", State: " + stoporder.State);
            if (stoporder.Account == Tool.AccountID && stoporder.SecCode == Tool.SecurityCode && stoporder.State == State.Completed && stoporder.LinkedOrder != 0)
            {
                await Task.Delay(20);
                bool check = Strategy.ProcessingExecutedStopOrders(stoporder);
                if (!check)
                {
                    PoolOrders.Add(new OrderInPool(stoporder));
                    Debug.WriteLine("OnOrder попал в Pool ! Comment: " + stoporder.Comment + ", OrderNum: " + stoporder.OrderNum + ", LinkedOrder: " + stoporder.LinkedOrder + ", State: " + stoporder.State);
                }
                if (stoporder.State == State.Completed && stoporder.LinkedOrder == 0)
                    Debug.WriteLine("пришел исполненный стопордер без связанного id");
            }

        }

        public void DistributionOrders()
        {
            for (int i = 0; i < PoolOrders.Count; i++)
            {
                if (PoolOrders[i].Order is Trade)
                {
                    bool check_execution = Strategy.ProcessingExecutedOrders((Trade)PoolOrders[i].Order);
                    if (check_execution)
                        PoolOrders[i].Distribution = check_execution;
                }
                if (PoolOrders[i].Order is StopOrder)
                {
                    bool check_execution = Strategy.ProcessingExecutedStopOrders((StopOrder)PoolOrders[i].Order);
                    if (check_execution)
                        PoolOrders[i].Distribution = check_execution;
                }
            }
            ClearPoolOrders();
        }

        private void ClearPoolOrders()
        {
            for (int i = 0; i < PoolOrders.Count; i++)
            {
                if (PoolOrders[i].Distribution)
                {
                    PoolOrders.RemoveAt(i);
                    i--;
                }
            }
        }

        #endregion

        #region Обработка сигналов от стратегии

        /// <summary>
        /// Обработчик события нового ордера.
        /// </summary>
        /// <param name="order"></param>
        public void NewOrder(OrderInfo order)
        {
            switch (order.Command)
            {
                case Command.Null:
                    break;
                case Command.SendOrder:
                    SendOrdersAsync(order);
                    break;
                case Command.SendStopLimitOrder:
                    SendStopLimitOrdersAsync(order);
                    break;
                case Command.SendTakeProfitOrder:
                    SendTakeProfitOrdersAsync(order);
                    break;
                case Command.SendTakeProfitAndStopLimitOrder:
                    SendTakeProfitAndStopLimitOrdersAsync(order);
                    break;
                case Command.TakeOffOrder:
                    TakeOffOrderAsync(order);
                    break;
                case Command.TakeOffStopLimitOrder:
                    TakeOffStopLimitOrderAsync(order);
                    break;
                case Command.TakeOffTakeProfitOrder:
                    TakeOffTakeProfitOrderAsync(order);
                    break;
                default:
                    break;

            }
        }

        #region Методы отправки и снятия ордеров

        /// <summary>
        /// Выставление ордеров.
        /// </summary>
        /// <param name="deal"></param>
        private async Task SendOrdersAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Completed)
                return;

            StopOrder temp_stoporder;
            temp_stoporder = await QuikConnecting.StopLimitOrder(Tool, 0m, 0m, order.Price, order.Price3, order.Operation, order.Vol, order.Comment);

            if (temp_stoporder.TransId > 0)
            {
                order.IDOrder = temp_stoporder.OrderNum;
                order.ExecutionStatus = State.Active;
                NewDeal?.Invoke(order);
            }
            else
            {
                //впилить защиту от не открытой сделки
            }
        }

        private async Task SendStopLimitOrdersAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Completed)
                return;

            StopOrder temp_stoporder;
            temp_stoporder = await QuikConnecting.StopLimitOrder(Tool, 0m, 0m, order.Price, order.Price, order.Operation, order.Vol, order.Comment);

            if (temp_stoporder.TransId > 0)
            {
                order.IDOrder = temp_stoporder.OrderNum;
                order.ExecutionStatus = State.Active;
                NewDeal?.Invoke(order);
            }
            else
            {
                //впилить защиту от не открытой сделки
            }
        }

        private async Task SendTakeProfitOrdersAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Completed)
                return;

            StopOrder temp_stoporder;
            temp_stoporder = await QuikConnecting.TakeProfitOrder(Tool, 0, 0, order.Price, order.Price, order.Operation, order.Vol, order.Comment);

            if (temp_stoporder.TransId > 0)
            {
                order.IDOrder = temp_stoporder.OrderNum;
                order.ExecutionStatus = State.Active;
                NewDeal?.Invoke(order);
            }
            else
            {
                //впилить защиту от не открытой сделки
            }
        }

        private async Task SendTakeProfitAndStopLimitOrdersAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Completed)
                return;

            StopOrder temp_stoporder;
            temp_stoporder = await QuikConnecting.TakeProfitStotLimitOrder(Tool, 0, 0, order.Price, order.Price2,
                order.Price3, order.Operation, order.Vol, order.Comment);

            if (temp_stoporder.TransId > 0)
            {
                order.IDOrder = temp_stoporder.OrderNum;
                order.ExecutionStatus = State.Active;
                NewDeal?.Invoke(order);
            }
            else
            {
                //впилить защиту от не открытой сделки
            }
        }

        private async Task TakeOffOrderAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Canceled && (order.TypeOrder == TypeOrder.LimitOrder || order.TypeOrder == TypeOrder.MarketOrder))
            {
                await QuikConnecting.TakeOffOrder(order.IDOrder);
                NewDeal?.Invoke(order);
            }

            if (order.ExecutionStatus == State.Canceled && (order.TypeOrder == TypeOrder.TakeProfit || order.TypeOrder == TypeOrder.StopLimit))
            {
                await QuikConnecting.TakeOffStopOrder(order.IDOrder);
                NewDeal?.Invoke(order);
            }

            if (order.ExecutionStatus == State.Canceled && order.IDLinkedOrder > 0
                && (order.TypeOrder == TypeOrder.TakeProfit || order.TypeOrder == TypeOrder.StopLimit))
            {
                await QuikConnecting.TakeOffOrder(order.IDLinkedOrder);
                NewDeal?.Invoke(order);
            }
        }

        private async Task TakeOffStopLimitOrderAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Canceled)
            {
                await QuikConnecting.TakeOffStopOrder(order.IDOrder);
                NewDeal?.Invoke(order);
            }

            if (order.ExecutionStatus == State.Canceled && order.IDLinkedOrder > 0)
            {
                await QuikConnecting.TakeOffOrder(order.IDLinkedOrder);
                NewDeal?.Invoke(order);
            }
        }

        private async Task TakeOffTakeProfitOrderAsync(OrderInfo order)
        {
            if (order.ExecutionStatus == State.Canceled)
            {
                await QuikConnecting.TakeOffStopOrder(order.IDOrder);
                NewDeal?.Invoke(order);
            }

            if (order.ExecutionStatus == State.Canceled && order.IDLinkedOrder > 0)
            {
                await QuikConnecting.TakeOffOrder(order.IDLinkedOrder);
                NewDeal?.Invoke(order);
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
