using QuikSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class Tool
    {
        private Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        private Quik quik;
        private string name;
        private string securityCode;
        private string classCode;

        //string clientCode;
        private string accountID;
        private string firmID;
        private int lot;
        private int priceAccuracy;
        private double guaranteeProviding;
        private decimal priceStep;
        private decimal step;
        private decimal lastPrice;

        #region Свойства
        /// <summary>
        /// Краткое наименование инструмента (бумаги)
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// Код инструмента (бумаги)
        /// </summary>
        public string SecurityCode { get { return securityCode; } }
        /// <summary>
        /// Код класса инструмента (бумаги)
        /// </summary>
        public string ClassCode { get { return classCode; } }
        /// <summary>
        /// Счет клиента
        /// </summary>
        public string AccountID { get { return accountID; } }
        /// <summary>
        /// Код фирмы
        /// </summary>
        public string FirmID { get { return firmID; } }
        /// <summary>
        /// Количество акций в одном лоте
        /// Для инструментов класса SPBFUT = 1
        /// </summary>
        public int Lot { get { return lot; } }
        /// <summary>
        /// Точность цены (количество знаков после запятой)
        /// </summary>
        public int PriceAccuracy { get { return priceAccuracy; } }
        /// <summary>
        /// Шаг цены
        /// </summary>
        public decimal Step { get { return step; } }
        /// <summary>
        /// Гарантийное обеспечение (только для срочного рынка)
        /// для фондовой секции = 0
        /// </summary>
        public double GuaranteeProviding { get { return guaranteeProviding; } }
        /// <summary>
        /// Стоимость шага цены
        /// </summary>
        public decimal PriceStep { get { return priceStep; } }
        /// <summary>
        /// Цена последней сделки
        /// </summary>
        public decimal LastPrice
        {
            get
            {
                lastPrice = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, "LAST").Result.ParamValue.Replace('.', separator));
                return lastPrice;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="quik"></param>
        /// <param name="securityCode">Код инструмента</param>
        /// <param name="classCode">Код класса</param>
        /// <param name="koefSlip">Коэффициент проскальзывания</param>
        public Tool(ref Quik quik, string securityCode, string classCode)
        {
            this.quik = quik;
            GetBaseParam(quik, securityCode, classCode);
        }

        private void GetBaseParam(Quik quik, string secCode, string class_code)
        {
            try
            {
                securityCode = secCode;
                classCode = class_code;
                if (quik != null)
                {
                    if (classCode != null && classCode != "")
                    {
                        try
                        {
                            name = quik.Class.GetSecurityInfo(classCode, securityCode).Result.ShortName;
                            accountID = quik.Class.GetTradeAccount(classCode).Result;
                            firmID = quik.Class.GetClassInfo(classCode).Result.FirmId;
                            step = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, "SEC_PRICE_STEP").Result.ParamValue.Replace('.', separator));
                            priceAccuracy = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(classCode, securityCode, "SEC_SCALE").Result.ParamValue.Replace('.', separator)));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Tool.GetBaseParam. Ошибка получения наименования для " + securityCode + ": " + e.Message);
                        }

                        if (classCode == "SPBFUT")
                        {
                            Console.WriteLine("Получаем 'guaranteeProviding'.");
                            lot = 1;
                            guaranteeProviding = Convert.ToDouble(quik.Trading.GetParamEx(classCode, securityCode, "BUYDEPO").Result.ParamValue.Replace('.', separator));
                        }
                        else
                        {
                            Console.WriteLine("Получаем 'lot'.");
                            lot = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(classCode, securityCode, "LOTSIZE").Result.ParamValue.Replace('.', separator)));
                            guaranteeProviding = 0;
                        }
                        try
                        {
                            priceStep = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, "STEPPRICET").Result.ParamValue.Replace('.', separator));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Instrument.GetBaseParam. Ошибка получения priceStep для " + securityCode + ": " + e.Message);
                            priceStep = 0;
                        }
                        if (priceStep == 0) priceStep = step;
                    }
                    else
                    {
                        Console.WriteLine("Tool.GetBaseParam. Ошибка: classCode не определен.");
                        lot = 0;
                        guaranteeProviding = 0;
                    }
                }
                else
                {
                    Console.WriteLine("Tool.GetBaseParam. quik = null !");
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Ошибка NullReferenceException в методе GetBaseParam: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка в методе GetBaseParam: " + e.Message);
            }
        }
    }
}
