using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Logic.Base
{
    public abstract class Strategy
    {
        public delegate void OnNewOrder(Deal deal);
        public abstract event OnNewOrder NewOrder;

        public abstract void AnalysisCandle(Candle candle);
        public abstract void AnalysisTick(Tick tick);
    }
}
