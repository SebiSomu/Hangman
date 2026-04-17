using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        private readonly User _user;
        private readonly IUserService _userService;

        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;

        public event Action<bool>? CloseRequested;

        public string UsernameDisplay => $"Enter password for {_user.Username}:";

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public PasswordDialogViewModel(User user, IUserService userService)
        {
            _user = user;
            _userService = userService;

            OkCommand = new RelayCommand(_ => Confirm());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(false));
        }

        private void Confirm()
        {
            if (_userService.ValidateUser(_user.Username, Password))
            {
                HasError = false;
                CloseRequested?.Invoke(true);
            }
            else
            {
                ErrorMessage = "Incorrect password. Please try again.";
                HasError = true;
            }
        }
    }
}
