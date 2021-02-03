using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public class StockData
    {
        public virtual int ID { get; set; }
        public DateTime DateTime { get; set; }
        public int Vol { get; set; }
    }
}
