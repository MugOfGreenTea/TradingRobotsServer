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
}
