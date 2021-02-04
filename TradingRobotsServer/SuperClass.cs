using QuikSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Support;

namespace TradingRobotsServer
{
    class SuperClass
    {
        QuikConnect QuikConnect;
        ManagementOfRisks ManagementOfRisks;

        public SuperClass(int port, string host, string name_tool, string name_strategy)
        {

        }
    }
}
