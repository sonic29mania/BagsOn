using System.Windows.Input;
using BagsOn.Commands;
using BagsOn.Views;

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

        public MainViewModel()
        {
          
            ShowAnalyticsCommand = new RelayCommand(_ => CurrentView = new AnalyticsView());
            ShowReportsCommand = new RelayCommand(_ => CurrentView = new ReportsView());
            ShowClientsCommand = new RelayCommand(_ => CurrentView = new ClientsView());

         
        }
    }
}
