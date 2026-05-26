using System.Windows.Input;
using BagsOn.Commands;
using BagsOn.Views;


// Клас MainViewModel відповідає за керування головним вмістом вікна програми.
// Він зберігає поточне відображення та містить команди для переходу між основними розділами застосунку.
namespace BagsOn.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowAssortmentCommand { get; }
        public ICommand ShowWarehouseCommand { get; }
        public ICommand ShowAnalyticsCommand { get; }
        public ICommand ShowReportsCommand { get; }
        public ICommand ShowClientsCommand { get; }

        // Конструктор MainViewModel створює команди навігації для відкриття потрібних сторінок у головному вікні.
        // Кожна команда змінює властивість CurrentView на відповідний View.
        public MainViewModel()
        {
          
            ShowAnalyticsCommand = new RelayCommand(_ => CurrentView = new AnalyticsView());

        }
    }
}
