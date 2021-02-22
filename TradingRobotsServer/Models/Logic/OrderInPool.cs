namespace TradingRobotsServer.Models.Logic
{
    public class OrderInPool
    {
        public object Order;
        public bool Distribution;

        public OrderInPool(object order)
        {
            Order = order;
            Distribution = false;
        }
    }
}
