using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public enum TypePoint : byte
    {
        Null,
        XPoint,
        YPoint,
        DateTimePoint
    }
    public enum TypeCandle : byte
    {
        Growing,
        Falling
    }
    public enum Extremum : byte
    {
        Null,
        Min,
        Max
    }
    public enum TypeOrder : byte
    {
        Null,
        LimitOrder,
        MarketOrder,
        TakeProfit,
        StopLimit,
        TakeProfitAndStopLimit
    }
    public enum StatusDeal : byte
    {
        None,
        WaitingOpen,
        Open,
        Closed,
        ClosedPrematurely
    }
    public enum StatusOrder : byte
    {
        Null,
        Active,
        Executed,
        PartiallyExecuted,
        Removed
    }
    public enum Command : byte
    {
        Null,
        SendOrder,
        SendStopLimitOrder,
        SendTakeProfitOrder,
        SendTakeProfitAndStopLimitOrder,
        TakeOffOrder,
        TakeOffStopLimitOrder,
        TakeOffTakeProfitOrder
    }
    public enum LogType : byte
    {
        Null,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
    public enum StatusClearing 
    {
        /// <summary>
        /// Не определено
        /// </summary>
        Null,
        /// <summary>
        /// Основная сессия
        /// </summary>
        MainSession,
        /// <summary>
        /// Начался промклиринг
        /// </summary>
        IndustrialClearingStarted,
        /// <summary>
        /// Завершился промклиринг, началась основная сессия
        /// </summary>
        IndustrialClearingCompleted,
        /// <summary>
        /// Начался основной клиринг
        /// </summary>
        MainClearingStarted,
        /// <summary>
        /// Основной клиринг: новая сессия назначена
        /// </summary>
        MainClearingNewSessionIsScheduled,
        /// <summary>
        /// Завершился основной клиринг, началась вечерняя сессия
        /// </summary>
        MainClearingCompleted,
        /// <summary>
        /// Завершилась вечерняя сессия
        /// </summary>
        EveningSessionEnded
    }
}
