using System;
using System.Windows.Input;

namespace AutoCAD_Version.WpfHelpers
{
    class GenericCommand<T> : ICommand
    {
        Action<T> executeCommand = null;
        Predicate<T> canExecute = null;

        public GenericCommand(Action<T> executeCommandAct, Predicate<T> canExecutePred)
        {
            executeCommand = executeCommandAct;
            canExecute = canExecutePred;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T tType)
                return canExecute(tType);
            else
                return false;
        }

        public void Execute(object parameter) => executeCommand((T)parameter);
    }

}
