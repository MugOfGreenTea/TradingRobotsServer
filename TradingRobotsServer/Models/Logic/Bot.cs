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

        public delegate void OnNewDeal(Deal deal, Command command);
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

        /// <summary>
        /// Обработчик события получение информации о новой сделке.
        /// </summary>
        /// <param name="tick"></param>
        private void OnTrade(Trade trade)
        {

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

        private long temp_id_order = -1;
        /// <summary>
        /// Обработчик события получение информации о новой заявке или изменения выставенной заявки.
        /// </summary>
        /// <param name="tick"></param>
        private async void OnOrder(Order order)
        {
            if (order.Account == Tool.AccountID && order.SecCode == Tool.SecurityCode && order.State == State.Completed && order.OrderNum != temp_id_order)
            {
                temp_id_order = order.OrderNum;
                //await Task.Delay(20);
                bool check = Strategy.ProcessingExecutedOrders(order);
                if (!check)
                {
                    PoolOrders.Add(new OrderInPool(order));
                    Debug.WriteLine("OnOrder попал в Pool");
                }
            }
        }

        private long temp_id_stoporder = -1;
        /// <summary>
        /// Обработчик события изменение позиции в стоп-заявках. 
        /// </summary>
        /// <param name="stoporder"></param>
        private async void OnStopOrder(StopOrder stoporder)
        {
            if (stoporder.OrderNum != temp_id_stoporder && stoporder.Account == Tool.AccountID && stoporder.SecCode == Tool.SecurityCode && stoporder.State == State.Completed && stoporder.LinkedOrder != 0)
            {
                temp_id_stoporder = stoporder.OrderNum;
                //await Task.Delay(20);
                bool check = Strategy.ProcessingExecutedStopOrders(stoporder);
                if (!check)
                {
                    PoolOrders.Add(new OrderInPool(stoporder));
                    Debug.WriteLine("OnOrder попал в Pool");
                }
                if (stoporder.State == State.Completed && stoporder.LinkedOrder == 0)
                    Debug.WriteLine("пришел исполненный стопордер без связанного id");
            }
        }

        public void DistributionOrders()
        {
            for (int i = 0; i < PoolOrders.Count; i++)
            {
                if (PoolOrders[i].Order is Order)
                {
                    bool check_execution = Strategy.ProcessingExecutedOrders((Order)PoolOrders[i].Order);
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
        public void NewOrder(Deal deal, Command command)
        {
            DebugLog("Ордер получен в Bot");

            switch (command)
            {
                case Command.Null:
                    break;
                case Command.SendOrder:
                    SendOrdersAsync(deal);
                    break;
                case Command.SendStopLimitOrder:
                    SendStopLimitOrdersAsync(deal);
                    break;
                case Command.SendTakeProfitOrder:
                    SendTakeProfitOrdersAsync(deal);
                    break;
                case Command.SendTakeProfitAndStopLimitOrder:
                    SendTakeProfitAndStopLimitOrdersAsync(deal);
                    break;
                case Command.TakeOffOrder:
                    TakeOffOrderAsync(deal);
                    break;
                case Command.TakeOffStopLimitOrder:
                    TakeOffStopLimitOrderAsync(deal);
                    break;
                case Command.TakeOffTakeProfitOrder:
                    TakeOffTakeProfitOrderAsync(deal);
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
        private async Task SendOrdersAsync(Deal deal)
        {
            for (int i = 0; i < deal.OrdersInfo.Count; i++)
            {
                if (deal.OrdersInfo[i].IssueStatus == State.Completed || deal.OrdersInfo[i].IssueStatus == State.Canceled)
                    continue;

                StopOrder temp_stoporder;
                if (deal.OrdersInfo[i].Vol == -1)
                    deal.OrdersInfo[i].Vol = 3;/*ManagementOfRisks.CalculationCountLotsToTradeFutures(QuikConnecting, Tool, deal, deal.OrdersInfo[i].Price, deal.StopLoss, 1, Tool.Lot, 3);*/

                temp_stoporder = await QuikConnecting.StopLimitOrder(Tool, 0m, 0m, deal.OrdersInfo[i].Price, deal.OrdersInfo[i].Price3, deal.OrdersInfo[i].Operation, deal.OrdersInfo[i].Vol, deal.OrdersInfo[i].Comment);

                if (temp_stoporder.TransId > 0)
                {
                    deal.OrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                    deal.OrdersInfo[i].IssueStatus = State.Completed;
                    NewDeal?.Invoke(deal, Command.SendOrder);
                }
                else
                {
                    //впилить защиту от не открытой сделки
                }
            }
        }

        private async Task SendStopLimitOrdersAsync(Deal deal)
        {
            if (deal.Status == StatusDeal.Closed)
                return;

            for (int i = 0; i < deal.StopLimitOrdersInfo.Count; i++)
            {
                if (deal.StopLimitOrdersInfo[i].IssueStatus == State.Completed || deal.StopLimitOrdersInfo[i].IssueStatus == State.Canceled)
                    continue;
                StopOrder temp_stoporder;
                deal.StopLimitOrdersInfo[i].Vol = deal.Vol;
                temp_stoporder = await QuikConnecting.StopLimitOrder(Tool, 0m, 0m, deal.StopLimitOrdersInfo[i].Price, deal.StopLimitOrdersInfo[i].Price,
                    deal.StopLimitOrdersInfo[i].Operation, deal.StopLimitOrdersInfo[i].Vol, deal.StopLimitOrdersInfo[i].Comment);

                if (temp_stoporder.TransId > 0)
                {
                    deal.StopLimitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                    deal.StopLimitOrdersInfo[i].IssueStatus = State.Completed;
                    deal.StopLimitOrdersInfo[i].ExecutionStatus = State.Active;
                    NewDeal?.Invoke(deal, Command.SendStopLimitOrder);
                }
                else
                {
                    //впилить защиту от не открытой сделки
                }
            }
        }

        private async Task SendTakeProfitOrdersAsync(Deal deal)
        {
            if (deal.Status == StatusDeal.Closed)
                return;

            for (int i = 0; i < deal.TakeProfitOrdersInfo.Count; i++)
            {
                if (deal.TakeProfitOrdersInfo[i].IssueStatus == State.Completed || deal.TakeProfitOrdersInfo[i].IssueStatus == State.Canceled)
                    continue;
                StopOrder temp_stoporder;
                if (deal.TakeProfitOrdersInfo[i].Vol == -1)
                    deal.TakeProfitOrdersInfo[i].Vol = ManagementOfRisks.CalculationCountPartSale(deal, 3, Tool.Lot);
                temp_stoporder = await QuikConnecting.TakeProfitOrder(Tool, 0, 0, deal.TakeProfitOrdersInfo[i].Price, deal.TakeProfitOrdersInfo[i].Price,
                    deal.TakeProfitOrdersInfo[i].Operation, 1/*deal.TakeProfitOrdersInfo[i].Vol*/, deal.TakeProfitOrdersInfo[i].Comment);

                if (temp_stoporder.TransId > 0)
                {
                    deal.TakeProfitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                    deal.TakeProfitOrdersInfo[i].IssueStatus = State.Completed;
                    deal.TakeProfitOrdersInfo[i].ExecutionStatus = State.Active;
                    NewDeal?.Invoke(deal, Command.SendTakeProfitOrder);
                }
                else
                {
                    //впилить защиту от не открытой сделки
                }
            }
        }

        private async Task SendTakeProfitAndStopLimitOrdersAsync(Deal deal)
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
                        temp_stoporder = await QuikConnecting.TakeProfitStotLimitOrder(Tool, 0, 0, deal.TakeProfitAndStopLimitOrdersInfo[i].Price, deal.TakeProfitAndStopLimitOrdersInfo[i].Price2,
                            deal.TakeProfitAndStopLimitOrdersInfo[i].Price3, deal.TakeProfitAndStopLimitOrdersInfo[i].Operation, deal.TakeProfitAndStopLimitOrdersInfo[i].Vol, deal.TakeProfitAndStopLimitOrdersInfo[i].Comment);

                        if (temp_stoporder.TransId > 0)
                        {
                            deal.TakeProfitAndStopLimitOrdersInfo[i].IDOrder = temp_stoporder.OrderNum;
                            deal.TakeProfitAndStopLimitOrdersInfo[i].IssueStatus = State.Completed;
                            NewDeal?.Invoke(deal, Command.SendTakeProfitAndStopLimitOrder);
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

        private async Task TakeOffOrderAsync(Deal deal)
        {
            for (int i = 0; i < deal.OrdersInfo.Count; i++)
            {
                if (deal.OrdersInfo[i].ExecutionStatus == State.Canceled && (deal.OrdersInfo[i].TypeOrder == TypeOrder.LimitOrder || deal.OrdersInfo[i].TypeOrder == TypeOrder.MarketOrder))
                {
                    await QuikConnecting.TakeOffOrder(deal.OrdersInfo[i].IDOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffOrder);
                }

                if (deal.OrdersInfo[i].ExecutionStatus == State.Canceled && (deal.OrdersInfo[i].TypeOrder == TypeOrder.TakeProfit || deal.OrdersInfo[i].TypeOrder == TypeOrder.StopLimit))
                {
                    await QuikConnecting.TakeOffStopOrder(deal.OrdersInfo[i].IDOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffOrder);
                }

                if(deal.OrdersInfo[i].ExecutionStatus == State.Canceled && deal.OrdersInfo[i].IDLinkedOrder > 0 
                    && (deal.OrdersInfo[i].TypeOrder == TypeOrder.TakeProfit || deal.OrdersInfo[i].TypeOrder == TypeOrder.StopLimit))
                {
                    await QuikConnecting.TakeOffOrder(deal.OrdersInfo[i].IDLinkedOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffOrder);
                }
            }
        }

        private async Task TakeOffStopLimitOrderAsync(Deal deal)
        {
            for (int i = 0; i < deal.StopLimitOrdersInfo.Count; i++)
            {
                if (deal.StopLimitOrdersInfo[i].ExecutionStatus == State.Canceled)
                {
                    await QuikConnecting.TakeOffStopOrder(deal.StopLimitOrdersInfo[i].IDOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffStopLimitOrder);
                }

                if (deal.StopLimitOrdersInfo[i].ExecutionStatus == State.Canceled && deal.StopLimitOrdersInfo[i].IDLinkedOrder > 0)
                {
                    await QuikConnecting.TakeOffOrder(deal.StopLimitOrdersInfo[i].IDLinkedOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffStopLimitOrder);
                }
            }
        }

        private async Task TakeOffTakeProfitOrderAsync(Deal deal)
        {
            for (int i = 0; i < deal.TakeProfitOrdersInfo.Count; i++)
            {
                if (deal.TakeProfitOrdersInfo[i].ExecutionStatus == State.Canceled)
                {
                    await QuikConnecting.TakeOffStopOrder(deal.TakeProfitOrdersInfo[i].IDOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffTakeProfitOrder);
                }

                if (deal.TakeProfitOrdersInfo[i].ExecutionStatus == State.Canceled && deal.TakeProfitOrdersInfo[i].IDLinkedOrder > 0)
                {
                    await QuikConnecting.TakeOffOrder(deal.TakeProfitOrdersInfo[i].IDLinkedOrder);
                    NewDeal?.Invoke(deal, Command.TakeOffTakeProfitOrder);
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
