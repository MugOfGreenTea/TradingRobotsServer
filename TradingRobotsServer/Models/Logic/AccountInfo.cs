namespace TradingRobotsServer.Models.Logic
{
    public class AccountInfo 
    {
        private decimal deposit;
        public decimal Deposite
        {
            get
            {
               return deposit; 
            }
            set => deposit = value;
        }

        private int position;

        private decimal limit_clear_position;
    }
}
