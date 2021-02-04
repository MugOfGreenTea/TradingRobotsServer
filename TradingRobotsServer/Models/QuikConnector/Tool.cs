using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using Candle = TradingRobotsServer.Models.Structures.Candle;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class Tool
    {
        private Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        private Quik quik;

        //string clientCode;

        private int id_tool;
        public int IDTool => id_tool;

        private string name;
        /// <summary>
        /// Краткое наименование инструмента (бумаги)
        /// </summary>
        public string Name => name;

        private string securityCode;
        /// <summary>
        /// Код инструмента (бумаги)
        /// </summary>
        public string SecurityCode => securityCode;

        private string classCode;
        /// <summary>
        /// Код класса инструмента (бумаги)
        /// </summary>
        public string ClassCode => classCode;

        private string accountID;
        /// <summary>
        /// Счет клиента
        /// </summary>
        public string AccountID => accountID;

        private string firmID;
        /// <summary>
        /// Код фирмы
        /// </summary>
        public string FirmID => firmID;

        private int lot;
        /// <summary>
        /// Количество акций в одном лоте
        /// Для инструментов класса SPBFUT = 1
        /// </summary>
        public int Lot => lot;

        private int priceAccuracy;
        /// <summary>
        /// Точность цены (количество знаков после запятой)
        /// </summary>
        public int PriceAccuracy => priceAccuracy;

        private decimal step;
        /// <summary>
        /// Шаг цены
        /// </summary>
        public decimal Step => step;

        private double guaranteeProviding;
        /// <summary>
        /// Гарантийное обеспечение (только для срочного рынка) для фондовой секции = 0
        /// </summary>
        public double GuaranteeProviding => guaranteeProviding;

        private decimal priceStep;
        /// <summary>
        /// Стоимость шага цены
        /// </summary>
        public decimal PriceStep => priceStep;

        private decimal lastPrice;
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

        private CandleInterval interval;
        /// <summary>
        /// Интервал инструмента.
        /// </summary>
        public CandleInterval Interval => interval;

        private List<Candle> candles;
        /// <summary>
        /// Список котировок.
        /// </summary>
        public List<Candle> Candles => candles;

        public bool ToolCreated = false;
        public bool isSubscribedToolOrderBook = false;
        public bool isSubscribedToolCandles = false;

        public Tool(ref Quik quik)
        {
            this.quik = quik;
            candles = new List<Candle>();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="quik"></param>
        /// <param name="securityCode">Код инструмента</param>
        /// <param name="classCode">Код класса</param>
        /// <param name="koefSlip">Коэффициент проскальзывания</param>
        public Tool(ref Quik quik, int id, string securityCode, string classCode, CandleInterval candle_interval)
        {
            this.quik = quik;
            id_tool = id;
            interval = candle_interval;
            candles = new List<Candle>();
            GetBaseParam(securityCode, classCode);
        }

        private void GetBaseParam(string secCode, string class_code)
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
                        if (priceStep == 0) 
                            priceStep = step;
                        ToolCreated = true;
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

        public void AddNewCandle(Candle candle)
        {
            candles.Add(candle);
        }
    }
}
