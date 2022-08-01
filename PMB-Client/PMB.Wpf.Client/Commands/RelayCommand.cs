using System;
using System.Windows.Input;

namespace PMB.Wpf.Client.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool> _func;
        
        public RelayCommand(Action<T> action)
        {
            _action = action;
        }

        public RelayCommand(Action<T> action, Func<T, bool> func)
        {
            _action = action;
            _func = func;
        }

        public bool CanExecute(object parameter) => _func == null || _func((T)parameter);

        public void Execute(object parameter) => _action((T)parameter);

        public event EventHandler CanExecuteChanged;
    }
}
