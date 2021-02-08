using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Structures
{
    public class OrderInfo
    {
        public TypeOrder TypeOrder;
        public decimal Price;
        public int Vol;
        public Operation Operation;
        public StatusOrder StatusOrder;

        public OrderInfo(TypeOrder type, decimal price, int vol, Operation operation)
        {
            TypeOrder = type;
            Price = price;
            Vol = vol;
            Operation = operation;
        }
    }
}
