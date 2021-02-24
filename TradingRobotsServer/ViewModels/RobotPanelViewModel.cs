using QuikSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TestQuotes.Infrastructure.Commands;
using TestQuotes.ViewModels.Base;
using TradingRobotsServer.Models;
using TradingRobotsServer.Models.Structures;
using TradingRobotsServer.View;

namespace TradingRobotsServer.ViewModels
{
    public class RobotPanelViewModel : ViewModel
    {
        #region - Переменные

        private int IDActivetab { get; set; } = -1;

        //public ObservableCollection<UserTabControl> Tabs { get; set; }

        public TabStrategyViewModel tabStrategyViewModel { get; set; }

        #endregion

        #region - Команды

        public ICommand CreateTab { get; }
        public ICommand LogCommand { get; }
        public RobotPanelViewModel()
        {
            CreateTab = new RelayCommand(On_Button_Execute, Can_Button_Execute);
            LogCommand = new RelayCommand(On_LogCommand_Execute, Can_LogCommand_Execute);

            //Tabs = new ObservableCollection<UserTabControl>();

            tabStrategyViewModel = new TabStrategyViewModel();

        }

        #endregion

        #region - Локальные переменные

        private TradingRobot robotSRH1;
        string param;
        #endregion

        #region - Обработчики событий формы

        private bool Can_Button_Execute(object obj)
        {
            return true;
        }
        private void On_Button_Execute(object obj)
        {
            //TradingRobot robotRIH1 = new TradingRobot();
            //param = "15;5;20;10,39,0;18,30,0;true;false";
            //robotRIH1.Run(Quik.DefaultPort, Quik.DefaultHost, "RIH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            //TradingRobot robotSiH1 = new TradingRobot();
            //param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotSiH1.Run(Quik.DefaultPort, Quik.DefaultHost, "SiH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            try
            {
                robotSRH1 = new TradingRobot();
                param = "15;5;2;10,39,0;18,30,0;true;false";
                robotSRH1.SetParam(Quik.DefaultPort, Quik.DefaultHost, "SiH1", QuikSharp.DataStructures.CandleInterval.M1, param);
            }
            catch
            {
                Logs.LogExcel(robotSRH1.Strategy.Deals);
            }
            //TradingRobot robotVBH1 = new TradingRobot();
            // param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotVBH1.Run(Quik.DefaultPort, Quik.DefaultHost, "VBH1", QuikSharp.DataStructures.CandleInterval.M1, param);

            //TradingRobot robotGZH1 = new TradingRobot();
            //param = "15;5;2;10,39,0;18,30,0;true;false";
            //robotGZH1.Run(Quik.DefaultPort, Quik.DefaultHost, "GZH1", QuikSharp.DataStructures.CandleInterval.M1, param);




        }

        private bool Can_LogCommand_Execute(object obj)
        {
            return true;
        }
        private void On_LogCommand_Execute(object obj)
        {
            Logs.LogExcel(tabStrategyViewModel.Robot.Strategy.Deals);
        }


        #endregion
    }

    public class UserTabControl
    {
        public int ID { get; set; }
        public string Header { get; set; }
        public TabStrategy Tab { get; set; }
    }
}
