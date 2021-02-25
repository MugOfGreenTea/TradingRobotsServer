using System;
using System.Collections.Generic;
using TradingRobotsServer.Models.QuikConnector;

namespace TradingRobotsServer.Models.Logic
{
    public class AccountInfo 
    {
        public QuikConnect quik_connect;

        private string firm_id;
        public string FirmID
        {
            get => firm_id;
            set => firm_id = value;
        }

        private List<string> accountID;
        /// <summary>
        /// Счета клиентов
        /// </summary>
        public List<string> AccountID
        {
            get => accountID;
            set => accountID = value;
        }

        private decimal deposit;
        public decimal Deposite
        {
            get
            {
                return deposit;/* = quik_connect.GetFuturesDepoClearLimit(tool.FirmID, tool.AccountID, 0, "SUR"); */
            }
            set => deposit = value;
        }

        private int position;
        public int Position
        {
            get => position;
            set => position = value;
        }

        private decimal limit_clear_position;
        public decimal LimitClearPosition
        {
            get => limit_clear_position;
            set => limit_clear_position = value;
        }

        public AccountInfo(string firm_id, List<string> account_id)
        {

        }
    }
}
