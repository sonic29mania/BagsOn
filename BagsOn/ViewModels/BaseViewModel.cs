using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BagsOn.ViewModels
{
    // Клас BaseViewModel є базовим класом для всіх ViewModel у проєкті.
    // Він реалізує інтерфейс INotifyPropertyChanged, щоб інтерфейс автоматично оновлювався при зміні властивостей.
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Метод OnPropertyChanged повідомляє інтерфейс користувача про зміну властивості.
        // Завдяки цьому елементи WPF, прив’язані через Binding, оновлюють свої значення.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}