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
    public enum Command
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
}
