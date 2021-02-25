using QuikSharp.DataStructures;

namespace TradingRobotsServer.Models.Structures
{
    public class OrderInfo
    {
        public int IDDeal;
        public TypeOrder TypeOrder;
        public Command Command;
        public decimal Price;
        public decimal Price2;//стоп-лимит, используется только в тейкпрофит-стоплимит заявках
        public decimal Price3;//цена заявки стоп-лимита, используется только в стоплимит и тейкпрофит-стоплимит заявках
        public int Vol;
        public Operation Operation;
        /// <summary>
        /// Статус исполнения.
        /// </summary>
        public State ExecutionStatus;
        /// <summary>
        /// Статус исполнения связанной заявки.
        /// </summary>
        public State ExecutionLinkedStatus;
        public long IDOrder;
        public long IDLinkedOrder;
        public string Comment;

        public OrderInfo(int id_deal, TypeOrder type, Command command, decimal price, int vol, Operation operation, string comment)
        {
            IDDeal = id_deal;
            TypeOrder = type;
            Command = command;
            Price = price;
            Vol = vol;
            Operation = operation;
            Comment = comment;
        }
        public OrderInfo(int id_deal, TypeOrder type, Command command, decimal price, decimal price3, int vol, Operation operation, string comment)
        {
            IDDeal = id_deal;
            TypeOrder = type;
            Command = command;
            Price = price;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            Comment = comment;
        }
        public OrderInfo(int id_deal, TypeOrder type, Command command, decimal price, decimal price2, decimal price3, int vol, Operation operation, string comment)
        {
            IDDeal = id_deal;
            TypeOrder = type;
            Command = command;
            Price = price;
            Price2 = price2;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            Comment = comment;
        }

        public OrderInfo(OrderInfo order)
        {
            IDDeal = order.IDDeal;
            TypeOrder = order.TypeOrder;
            Command = order.Command;
            Price = order.Price;
            Price2 = order.Price2;
            Price3 = order.Price3;
            Vol = order.Vol;
            Operation = order.Operation;
            ExecutionStatus = order.ExecutionStatus;
            IDOrder = order.IDOrder;
            IDLinkedOrder = order.IDLinkedOrder;
            Comment = order.Comment;
        }
    }
}
