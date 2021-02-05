using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Strategy
{
    public class Bot
    {
        Tool Tool;
        IStrategy Strategy;

        public Bot(Tool tool, IStrategy strategy)
        {
            Tool = tool;
            Strategy = strategy;

            Tool.NewCandle += NewCandle;
            Tool.NewTick += NewTick;
        }

        public void NewCandle(Candle candle)
        {
            Strategy.AnalysisCandle(candle);
        }

        public void NewTick(Tick tick)
        {
            Strategy.AnalysisTick(tick);
        }

        public void SendingOrder()
        {

        }
    }
}
