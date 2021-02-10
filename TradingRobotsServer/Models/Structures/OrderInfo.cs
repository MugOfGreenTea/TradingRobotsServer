using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Structures
{
    public class OrderInfo
    {
        public TypeOrder TypeOrder;
        public decimal Price;
        public int Vol;
        public Operation Operation;
        public State State;

        public OrderInfo(TypeOrder type, decimal price, int vol, Operation operation, State state)
        {
            TypeOrder = type;
            Price = price;
            Vol = vol;
            Operation = operation;
            State = state;
        }
    }
}
