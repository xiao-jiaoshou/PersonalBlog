using PersonalBlog.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PersonalBlog.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private int _selectedIndex;

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);
            SelectedIndex = 0;
            UpdateCurrentView();
        }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                UpdateCurrentView();
            }
        }

        public ICommand NavigateCommand { get; private set; }

        private void Navigate(object parameter)
        {
            
            SelectedIndex = int.Parse(parameter.ToString());
            
        }

        private void UpdateCurrentView()
        {
            switch (SelectedIndex)
            {
                case 0:
                    CurrentView = new ProfileView();
                    break;
                case 1:
                    CurrentView = new ArticlesView();
                    break;
                case 2:
                    CurrentView = new NotesView();
                    break;
                case 3:
                    CurrentView = new LinksView();
                    break;
                case 4:
                    CurrentView = new GuestbookView();
                    break;
                default:
                    CurrentView = new ProfileView();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
