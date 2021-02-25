using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TradingRobotsServer.Models.Structures;
using Candle = TradingRobotsServer.Models.Structures.Candle;

namespace TradingRobotsServer.Models.QuikConnector
{
    public class Tool
    {
        private Char separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
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

        private decimal guaranteeProvidingbuy;
        /// <summary>
        /// Гарантийное обеспечение покупателя (только для срочного рынка) для фондовой секции = 0
        /// </summary>
        public decimal GuaranteeProvidingBuy
        {
            get
            {
                guaranteeProvidingbuy = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.BUYDEPO).Result.ParamValue.Replace('.', separator));
                return guaranteeProvidingbuy;
            }
        }

        private decimal guaranteeProvidingsell;
        /// <summary>
        /// Гарантийное обеспечение продавца (только для срочного рынка) для фондовой секции = 0
        /// </summary>
        public decimal GuaranteeProvidingSell
        {
            get
            {
                guaranteeProvidingsell = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.BUYDEPO).Result.ParamValue.Replace('.', separator));
                return guaranteeProvidingsell;
            }
        }

        private decimal price_max;
        /// <summary>
        /// Максимальная возможная цена.
        /// </summary>
        public decimal PriceMax
        {
            get => price_max = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMAX).Result.ParamValue.Replace('.', separator));
            set => price_max = value;
        }

        private decimal price_min;
        /// <summary>
        /// Минимальная возможная цена.
        /// </summary>
        public decimal PriceMin
        {
            get => price_min = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMIN).Result.ParamValue.Replace('.', separator));
            set => price_min = value;
        }

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
                lastPrice = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.LAST).Result.ParamValue.Replace('.', separator));
                NewTick?.Invoke(new Tick(securityCode, classCode, interval, lastPrice));
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

        private DateTime start_morning_session;
        /// <summary>
        /// Начало утренней сессии.
        /// </summary>
        public DateTime StartMorningSession
        {
            get => start_morning_session;
            set => start_morning_session = value;
        }

        private DateTime end_morning_session;
        /// <summary>
        /// Конец утренней сессии.
        /// </summary>
        public DateTime EndMorningSession
        {
            get => end_morning_session;
            set => end_morning_session = value;
        }

        private DateTime start_main_session;
        /// <summary>
        /// Начало основной сессии.
        /// </summary>
        public DateTime StartMainSession
        {
            get => start_main_session;
            set => start_main_session = value;
        }

        private DateTime end_main_session;
        /// <summary>
        /// Конеч основной сессии.
        /// </summary>
        public DateTime EndMainSession
        {
            get => end_main_session;
            set => end_main_session = value;
        }

        private DateTime start_evening_session;
        /// <summary>
        /// Начало вечерней сессии.
        /// </summary>
        public DateTime StartEveningSession
        {
            get => start_evening_session;
            set => start_evening_session = value;
        }

        private DateTime end_evening_session;
        /// <summary>
        /// Конец вечерней сессии.
        /// </summary>
        public DateTime EndevEningSession
        {
            get => end_evening_session;
            set => end_evening_session = value;
        }

        private DateTime maturity_time;
        /// <summary>
        /// Время погашения.
        /// </summary>
        public DateTime MaturityTime
        {
            get => maturity_time;
            set => maturity_time = value;
        }

        private decimal status_clearing;
        /// <summary>
        /// Статус клиринга.
        /// </summary>
        public StatusClearing StatusClearing
        {
            get => (StatusClearing)status_clearing;
            set => status_clearing = Convert.ToDecimal(value);
        }

        private int position;
        public int Position
        {
            get => position = Convert.ToInt32(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.NUMCONTRACTS).Result.ParamValue.Replace('.', separator));
            set => position = value;
        }

        public bool ToolCreated = false;
        public bool isSubscribedToolOrderBook = false;
        public bool isSubscribedToolCandles = false;

        public delegate void OnNewCandle(Candle candle);
        public event OnNewCandle NewCandle;
        public delegate void OnNewTick(Tick tick);
        public event OnNewTick NewTick;

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
                            step = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.SEC_PRICE_STEP).Result.ParamValue.Replace('.', separator));
                            priceAccuracy = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.SEC_SCALE).Result.ParamValue.Replace('.', separator)));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Tool.GetBaseParam. Ошибка получения наименования для " + securityCode + ": " + e.Message);
                        }

                        if (classCode == "SPBFUT")
                        {
                            lot = 1;
                            guaranteeProvidingbuy = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.BUYDEPO).Result.ParamValue.Replace('.', separator));
                            guaranteeProvidingsell = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.SELLDEPO).Result.ParamValue.Replace('.', separator));

                            price_max = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMAX).Result.ParamValue.Replace('.', separator));
                            price_min = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMIN).Result.ParamValue.Replace('.', separator));

                            start_morning_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.MONSTARTTIME).Result.ParamImage.Replace('.', separator));
                            end_morning_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.MONSTARTTIME).Result.ParamImage.Replace('.', separator));
                            start_main_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.STARTTIME).Result.ParamImage.Replace('.', separator));
                            end_main_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.ENDTIME).Result.ParamImage.Replace('.', separator));
                            start_evening_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.EVNSTARTTIME).Result.ParamImage.Replace('.', separator));
                            end_evening_session = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.EVNENDTIME).Result.ParamImage.Replace('.', separator));

                            maturity_time = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.MAT_DATE).Result.ParamImage.Replace('.', separator));
                            status_clearing = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.CLSTATE).Result.ParamValue.Replace('.', separator));
                        }
                        else
                        {
                            lot = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.LOTSIZE).Result.ParamValue.Replace('.', separator)));
                            guaranteeProvidingbuy = 0;
                            guaranteeProvidingsell = 0;
                        }
                        try
                        {
                            priceStep = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.STEPPRICET).Result.ParamValue.Replace('.', separator));
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
                        guaranteeProvidingbuy = 0;
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

        public void UpdateParam()
        {
            guaranteeProvidingbuy = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.BUYDEPO).Result.ParamValue.Replace('.', separator));
            guaranteeProvidingsell = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.SELLDEPO).Result.ParamValue.Replace('.', separator));

            price_max = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMAX).Result.ParamValue.Replace('.', separator));
            price_min = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.PRICEMIN).Result.ParamValue.Replace('.', separator));

            maturity_time = Convert.ToDateTime(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.MAT_DATE).Result.ParamValue.Replace('.', separator));
            status_clearing = Convert.ToInt32(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.CLSTATE).Result.ParamValue.Replace('.', separator));
        }

        public decimal CallLastPrice()
        {
            lastPrice = Convert.ToDecimal(quik.Trading.GetParamEx(classCode, securityCode, ParamNames.LAST).Result.ParamValue.Replace('.', separator));
            NewTick?.Invoke(new Tick(securityCode, classCode, interval, lastPrice));
            return lastPrice;
        }

        public void SubscribeNewCandle()
        {
            quik.Candles.NewCandle += ToolNewCandle;
        }

        public void AddNewCandle(Candle candle)
        {
            if (Candles.Count != 0 && Candles.Count >= 200)
                candles.RemoveAt(0);
            candle.ID = Candles.Last().ID + 1;
            candles.Add(candle);

            NewCandle?.Invoke(candle);
        }

        public void ToolNewCandle(QuikSharp.DataStructures.Candle candle)
        {
            if (Candles != null && candle.SecCode == SecurityCode && candle.ClassCode == ClassCode && candle.Interval == Interval)
            {
                Candle temp_candle = new Candle(candle);
                if (Candles.Count != 0 && Candles.Count >= 1000)
                    candles.RemoveAt(0);
                temp_candle.ID = Candles.Count;
                candles.Add(temp_candle);

                NewCandle?.Invoke(temp_candle);
            }
        }

        public async void GetHistoricalCandlesAsync(int count)
        {
            List<QuikSharp.DataStructures.Candle> temp_candles = await quik.Candles.GetLastCandles(classCode, securityCode, interval, count);
            foreach (var candle in temp_candles)
            {
                Candle temp_candle = new Candle(candle);
                temp_candle.ID = Candles.Count;
                Candles.Add(temp_candle);
                NewCandle?.Invoke(Candles.Last());
            }
        }
    }
}
