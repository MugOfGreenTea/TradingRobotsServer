using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public class DataPoint
    {
        private int x;
        private decimal y;
        public int XPoint
        {
            get => x;
            set
            {
                x = value;
            }
        }
        public virtual decimal YPoint
        {
            get => y;
            set
            {
                y = value;
            }
        }

        public DataPoint()
        {

        }

        public DataPoint(int x, decimal y)
        {
            XPoint = x;
            YPoint = y;
        }

    }
}
