using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.Structures
{
    public class TrendDataPoint : DataPoint
    {
        private DateTime dateTime;
        public DateTime DateTime
        {
            get => dateTime;
            set
            {
                dateTime = value;
            }
        }

        public TrendDataPoint()
        {

        }

        public TrendDataPoint(int x, decimal y)
        {
            XPoint = x;
            YPoint = y;
        }

        public TrendDataPoint(int x, decimal y, DateTime date_time)
        {
            XPoint = x;
            YPoint = y;
            DateTime = date_time;
        }

        public TrendDataPoint(TrendDataPoint point)
        {
            XPoint = point.XPoint;
            YPoint = point.YPoint;
            DateTime = point.DateTime;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TrendDataPoint))
                return false;
            else
                return (
                    this.XPoint == ((TrendDataPoint)obj).XPoint
                    && this.YPoint == ((TrendDataPoint)obj).YPoint
                    && this.DateTime.Equals(((TrendDataPoint)obj).DateTime)
                    );
        }
    }
}
