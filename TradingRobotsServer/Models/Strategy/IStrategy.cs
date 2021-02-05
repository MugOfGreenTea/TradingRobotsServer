using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Strategy
{
    public interface IStrategy
    {
        public void AnalysisCandle(Candle candle);
        public void AnalysisTick(Tick tick);
    }
}
