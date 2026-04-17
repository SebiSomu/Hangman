using System.Windows.Input;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }

        public event EventHandler<User>? LoginSuccessful;
        public event EventHandler? SignUpRequested;

        public LoginViewModel(UserService userService)
        {
            _userService = userService;
            LoginCommand = new RelayCommand(_ => Login(), _ => CanLogin());
            SignUpCommand = new RelayCommand(_ => SignUpRequested?.Invoke(this, EventArgs.Empty));
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void Login()
        {
            if (!_userService.UserExists(Username))
            {
                ErrorMessage = "User does not exist.";
                return;
            }

            if (!_userService.ValidateUser(Username, Password))
            {
                ErrorMessage = "Incorrect password.";
                return;
            }

            var user = _userService.GetUser(Username);
            if (user != null)
            {
                LoginSuccessful?.Invoke(this, user);
            }
        }
    }
}
