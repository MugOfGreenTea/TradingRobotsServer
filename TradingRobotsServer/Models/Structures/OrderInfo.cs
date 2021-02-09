using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Structures
{
    public struct OrderInfo
    {
        public TypeOrder TypeOrder;
        public decimal Price;
        public int Vol;
        public Operation Operation;

        public OrderInfo(TypeOrder type, decimal price, int vol, Operation operation)
        {
            TypeOrder = type;
            Price = price;
            Vol = vol;
            Operation = operation;
        }
    }
}
