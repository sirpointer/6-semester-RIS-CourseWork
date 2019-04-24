using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeaBattleClient.ViewModels
{
    public class ActionCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public ActionCommand(Action execute) : this(execute, null) { }

        public ActionCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void ActionCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Может и не надо
    /// </summary>
    public class SetLocatoinCommand : ICommand
    {
        private readonly Action<Ship, Location> _execute;
        private readonly Func<Ship, Location, bool> _canExecute;

        public SetLocatoinCommand(Action<Ship, Location> execute) : this(execute, null) { }

        public SetLocatoinCommand(Action<Ship, Location> execute, Func<Ship, Location, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            //return _canExecute == null ? true : _canExecute(parameter as Ship, Location);
            return false;
        }

        public void Execute(object parameter)
        {
           // _execute(parameter as Ship, Location);
        }

        public void ActionCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
