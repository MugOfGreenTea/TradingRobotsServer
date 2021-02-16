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
        /// <summary>
        /// Статус выставления.
        /// </summary>
        public State IssueStatus;
        /// <summary>
        /// Статус исполнения.
        /// </summary>
        public State ExecutionStatus;
        /// <summary>
        /// Статус выставления связанной заявки.
        /// </summary>
        public State IssueLinkedStatus;
        /// <summary>
        /// Статус исполнения связаной заявки.
        /// </summary>
        public State ExecutionLinkedStatus;
        public long IDOrder;
        public long IDLinkedOrder;//id заявки связаной со стоп-заявкой

        public OrderInfo(TypeOrder type, decimal price, int vol, Operation operation, State issues_status, State execution_status)
        {
            TypeOrder = type;
            Price = price;
            Vol = vol;
            Operation = operation;
            IssueStatus = issues_status;
            ExecutionStatus = execution_status;
        }
        public OrderInfo(TypeOrder type, decimal price, decimal price3, int vol, Operation operation, State issues_status, State execution_status)
        {
            TypeOrder = type;
            Price = price;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            IssueStatus = issues_status;
            ExecutionStatus = execution_status;
        }
        public OrderInfo(TypeOrder type, decimal price, decimal price2, decimal price3, int vol, Operation operation, State issues_status, State execution_status)
        {
            TypeOrder = type;
            Price = price;
            Price2 = price2;
            Price3 = price3;
            Vol = vol;
            Operation = operation;
            IssueStatus = issues_status;
            ExecutionStatus = execution_status;
        }
    }
}
