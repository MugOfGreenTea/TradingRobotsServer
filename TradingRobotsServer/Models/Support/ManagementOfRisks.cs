using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingRobotsServer.Models.QuikConnector;
using TradingRobotsServer.Models.Structures;

namespace TradingRobotsServer.Models.Support
{
    public class ManagementOfRisks
    {
        public static int CalculationMaxCountVolFutures(Tool tool, decimal deposit, Operation operation, int count_lots_to_trade)
        {
            int count_vol_max;

            if (operation == Operation.Buy)
                count_vol_max = Convert.ToInt32(deposit / tool.GuaranteeProvidingBuy) - 2;
            else
                count_vol_max = Convert.ToInt32(deposit / tool.GuaranteeProvidingSell) - 2;

            if (count_lots_to_trade > count_vol_max)
                return count_vol_max;
            else
                return count_lots_to_trade;
        }

        public static int CalculationCountLotsToTradeFutures(QuikConnect quik_connect, Tool tool, Deal deal, decimal price, decimal stoploss, decimal risk_percent, int lot_size, int limiter_in_parts)
        {
            decimal Deposit = (quik_connect.GetFuturesDepoClearLimit(tool.FirmID, tool.AccountID, 0, "SUR")) / limiter_in_parts;

            decimal risk_price = Math.Abs(price - stoploss); //величина риска

            int amount_vol = (int)Math.Floor((Deposit / 100 * risk_percent) / risk_price); //
            int count_lots = (int)Math.Floor((decimal)amount_vol / lot_size) * lot_size; //количество лотов

            return CalculationMaxCountVolFutures(tool, Deposit, deal.Operation, count_lots);
        }

        public static int CalculationCountPartSale(int vol, int part, int lot_size)
        {
            return (int)Math.Floor((decimal)(vol / part) / lot_size) * lot_size;
        }
    }
}
