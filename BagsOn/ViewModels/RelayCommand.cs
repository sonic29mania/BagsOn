using System;
using System.Windows.Input;

namespace BagsOn.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        // Конструктор RelayCommand приймає дію, яку потрібно виконати, та необов’язкову умову доступності команди.
        // Це дозволяє задавати логіку натискання кнопки та перевірку, чи можна її виконати.
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        // Метод CanExecute визначає, чи може команда виконатися в поточний момент.
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        // Метод Execute виконує основну дію команди.
        // Саме цей метод запускається, коли користувач натискає кнопку або викликає команду з інтерфейсу.
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
        // Подія CanExecuteChanged повідомляє WPF, що доступність команди могла змінитися.
        // Вона підключена до CommandManager.RequerySuggested, тому інтерфейс може автоматично оновлювати стан кнопок.
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}