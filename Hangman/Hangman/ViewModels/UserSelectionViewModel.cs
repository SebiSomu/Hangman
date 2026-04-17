using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class UserSelectionViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private ObservableCollection<User> _users;
        private User? _selectedUser;
        private int _selectedIndex;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value) && value != null)
                {
                    _selectedIndex = Users.IndexOf(value);
                }
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (SetProperty(ref _selectedIndex, value) && value >= 0 && value < Users.Count)
                {
                    SelectedUser = Users[value];
                }
            }
        }

        public ICommand NewUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PreviousUserCommand { get; }
        public ICommand NextUserCommand { get; }

        public event EventHandler? NewUserRequested;
        public event EventHandler<User>? PlayRequested;
        public event EventHandler? CancelRequested;

        public UserSelectionViewModel(IUserService userService)
        {
            _userService = userService;
            Users = new ObservableCollection<User>(_userService.GetAllUsers());

            NewUserCommand = new RelayCommand(_ => NewUserRequested?.Invoke(this, EventArgs.Empty));
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null);
            PlayCommand = new RelayCommand(_ => Play(), _ => SelectedUser != null);
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));
            PreviousUserCommand = new RelayCommand(_ => NavigatePrevious(), _ => CanNavigatePrevious());
            NextUserCommand = new RelayCommand(_ => NavigateNext(), _ => CanNavigateNext());

            if (Users.Count > 0)
            {
                SelectedUser = Users[0];
                SelectedIndex = 0;
            }
        }

        private bool CanNavigatePrevious() => SelectedIndex > 0;
        private bool CanNavigateNext() => SelectedIndex < Users.Count - 1;

        private void NavigatePrevious()
        {
            if (CanNavigatePrevious())
            {
                SelectedIndex--;
            }
        }

        private void NavigateNext()
        {
            if (CanNavigateNext())
            {
                SelectedIndex++;
            }
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{SelectedUser.Username}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteUser(SelectedUser.Username);
                Users.Remove(SelectedUser);
                
                if (Users.Count > 0)
                {
                    SelectedIndex = Math.Min(SelectedIndex, Users.Count - 1);
                    SelectedUser = Users[SelectedIndex];
                }
                else
                {
                    SelectedUser = null;
                    SelectedIndex = -1;
                }
            }
        }

        private void Play()
        {
            if (SelectedUser != null)
            {
                PlayRequested?.Invoke(this, SelectedUser);
            }
        }
    }
}
