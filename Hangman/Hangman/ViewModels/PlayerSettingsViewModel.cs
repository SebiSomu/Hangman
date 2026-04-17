using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hangman.Models;
using Hangman.Services;
using Microsoft.Win32;

namespace Hangman.ViewModels
{
    public class PlayerSettingsViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAvatarService _avatarService;
        private User _currentUser;
        private string? _avatarFileName;
        private BitmapImage? _avatarImage;
        private string _errorMessage = string.Empty;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
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

        public ICommand ChangeImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public event EventHandler? SettingsSaved;
        public event EventHandler? BackRequested;

        public PlayerSettingsViewModel(User currentUser, IUserService userService, IAvatarService avatarService)
        {
            _currentUser = currentUser;
            _userService = userService;
            _avatarService = avatarService;
            AvatarFileName = currentUser.AvatarFileName;
            AvatarImage = _avatarService.GetAvatarImage(AvatarFileName);

            ChangeImageCommand = new RelayCommand(_ => ChangeImage());
            SaveCommand = new RelayCommand(_ => SaveSettings());
            BackCommand = new RelayCommand(_ => {
                if (AvatarFileName != _currentUser.AvatarFileName)
                {
                    _avatarService.DeleteAvatar(AvatarFileName);
                }
                BackRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private void ChangeImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*",
                Title = "Select a profile picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var newFileName = _avatarService.SaveAvatar(openFileDialog.FileName, _currentUser.Username);
                    
                    if (AvatarFileName != _currentUser.AvatarFileName)
                    {
                        _avatarService.DeleteAvatar(AvatarFileName);
                    }

                    AvatarFileName = newFileName;
                    AvatarImage = _avatarService.GetAvatarImage(newFileName);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error processing image: " + ex.Message;
                }
            }
        }

        private void SaveSettings()
        {
            if (_currentUser.AvatarFileName != AvatarFileName)
            {
                _avatarService.DeleteAvatar(_currentUser.AvatarFileName);
            }
            CurrentUser.AvatarFileName = AvatarFileName;
            _userService.UpdateUser(CurrentUser);
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}
