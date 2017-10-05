using System;
using System.Windows.Input;

namespace AutoCAD_Version.WpfHelpers
{
    /// <summary>
    /// Generic command to simplify using commands in View Models
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class GenericCommand<T> : ICommand
    {
        /// <summary>
        /// Local store for the Execute event
        /// </summary>
        private Action<T> executeCommand = null;
        /// <summary>
        /// Local store for the CanExecute event
        /// </summary>
        private Predicate<T> canExecute = null;

        /// <summary>
        /// Constructor that takes the execute and canExecute functions
        /// </summary>
        /// <param name="executeCommandAct">Executed when the Execute Event is fired</param>
        /// <param name="canExecutePred">Executed when the CanExecute Event is fired</param>
        public GenericCommand(Action<T> executeCommandAct, Predicate<T> canExecutePred)
        {
            executeCommand = executeCommandAct;
            canExecute = canExecutePred;
        }
        /// <summary>
        /// Handle CanExecuteChanged by calling RequerySuggested
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        /// <summary>
        /// ICommand Interface ICommand.CanExecute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            bool? result = false;

            if (parameter is T theRightType)
                result = canExecute?.Invoke(theRightType);

            return result ?? false;
        }

        /// <summary>
        /// ICommand Interface ICommand.Execute
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (parameter is T theRightType)
                executeCommand?.Invoke(theRightType);
        }
    }

}
