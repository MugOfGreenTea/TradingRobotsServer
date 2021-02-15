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
                count_vol_max = Convert.ToInt32(deposit / tool.GuaranteeProvidingBuy) - 1;
            else
                count_vol_max = Convert.ToInt32(deposit / tool.GuaranteeProvidingSell) - 1;

            if (count_lots_to_trade > count_vol_max)
                return count_vol_max;
            else
                return count_lots_to_trade;
        }

        public static int CalculationCountLotsToTradeFutures(QuikConnect quik_connect, Tool tool, Deal deal, decimal stoploss, decimal risk_percent, int lot_size)
        {
            decimal Deposit = quik_connect.GetFuturesDepoClearLimit(tool.FirmID, tool.AccountID, 0, "SUR");

            decimal trend_entry_price = deal.TradeEntryPoint.YPoint; //цена входа

            decimal risk_price = Math.Abs(trend_entry_price - stoploss); //величина риска

            int amount_vol = (int)Math.Floor((Deposit / 100 * risk_percent) / risk_price); //
            int count_lots = (int)Math.Floor((decimal)amount_vol / lot_size) * lot_size; //количество лотов

            return CalculationMaxCountVolFutures(tool, Deposit, deal.Operation, count_lots);
        }

        public static int CalculationCountPartSale(Deal deal, int part, int lot_size)
        {
            return (int)Math.Floor((decimal)(deal.Vol / part) / lot_size) * lot_size;
        }
    }
}
