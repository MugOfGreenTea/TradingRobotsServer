using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestQuotes.Infrastructure.Commands.Base;

namespace TestQuotes.Infrastructure.Commands
{
    class RelayCommand : Command
    {
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> can_execute = null)
        {
            _Execute = execute;
            _CanExecute = can_execute;
        }

        public override bool CanExecute(object parameter)
        {
            _CanExecute?.Invoke(parameter);
            return true;
        }

        public override void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }
}
