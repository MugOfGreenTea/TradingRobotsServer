using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Structures
{
    public class OrderInfo
    {
        public TypeOrder TypeOrder;
        public decimal Price;
        public decimal Price2;//стоп-лимит, используется только в тейкпрофит-стоплимит заявках
        public decimal Price3;//цена заявки стоп-лимита, используется только в стоплимит и тейкпрофит-стоплимит заявках
        public int Vol;
        public Operation Operation;
        public State IssueStatus;
        public State ExecutionStatus;
        public long IDOrder;
        public long IDLinkedOrder;//id заявки связаной со стоп-заявкой

        public OrderInfo(TypeOrder type, decimal price, int vol, Operation operation, State state)
        {
            TypeOrder = type;
            Price = price;
            Vol = vol;
            Operation = operation;
            IssueStatus = state;
        }
        public OrderInfo(TypeOrder type, decimal price, decimal price3, int vol, Operation operation, State state)
        {
            TypeOrder = type;
            Price = price;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            IssueStatus = state;
        }
        public OrderInfo(TypeOrder type, decimal price, decimal price2, decimal price3, int vol, Operation operation, State state)
        {
            TypeOrder = type;
            Price = price;
            Price2 = price2;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            IssueStatus = state;
        }
    }
}
