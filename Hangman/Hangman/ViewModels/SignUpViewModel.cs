using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hangman.Models;
using Hangman.Services;
using Microsoft.Win32;

namespace Hangman.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAvatarService _avatarService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string? _avatarFileName;
        private BitmapImage? _avatarImage;
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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string? AvatarFileName
        {
            get => _avatarFileName;
            set => SetProperty(ref _avatarFileName, value);
        }

        public BitmapImage? AvatarImage
        {
            get => _avatarImage;
            set => SetProperty(ref _avatarImage, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SignUpCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand BackCommand { get; }

        public event EventHandler? SignUpSuccessful;
        public event EventHandler? BackToLoginRequested;

        public SignUpViewModel(IUserService userService, IAvatarService avatarService)
        {
            _userService = userService;
            _avatarService = avatarService;
            SignUpCommand = new RelayCommand(_ => SignUp(), _ => CanSignUp());
            SelectImageCommand = new RelayCommand(_ => SelectImage());
            BackCommand = new RelayCommand(_ => BackToLoginRequested?.Invoke(this, EventArgs.Empty));
        }

        private bool CanSignUp()
        {
            return !string.IsNullOrWhiteSpace(Username) 
                   && !string.IsNullOrWhiteSpace(Password)
                   && !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private void SelectImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif",
                Title = "Select a profile picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var fileName = _avatarService.SaveAvatar(openFileDialog.FileName, Username);
                    AvatarFileName = fileName;

                    AvatarImage = _avatarService.GetAvatarImage(fileName);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error processing image: " + ex.Message;
                }
            }
        }

        private void SignUp()
        {
            if (Username.Trim().Contains(" "))
            {
                ErrorMessage = "Username must be a single word.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            if (_userService.UserExists(Username))
            {
                ErrorMessage = "Username already taken";
                return;
            }

            var newUser = new User(Username, Password, AvatarFileName);
            _userService.AddUser(newUser);
            SignUpSuccessful?.Invoke(this, EventArgs.Empty);
        }
    }
}
