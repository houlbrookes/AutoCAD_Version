using System;
using System.Windows.Input;

namespace AutoCAD_VersioChecker.WpfHelpers
{
    class GenericCommand<T> : ICommand
    {
        Action<T> GetAutoCADFolder = null;
        Predicate<T> canExecute = null;

        public GenericCommand(Action<T> GetAutoCADFolder, Predicate<T> canExecute)
        {
            this.GetAutoCADFolder = GetAutoCADFolder;
            this.canExecute = canExecute;
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

        public void Execute(object parameter) => GetAutoCADFolder((T)parameter);
    }

}
