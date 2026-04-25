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
        private string? _tempAvatarFileName;
        private BitmapImage? _avatarImage;
        private string _errorMessage = string.Empty;
        private string _editableUsername;

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

        public string? TempAvatarFileName
        {
            get => _tempAvatarFileName;
            set => SetProperty(ref _tempAvatarFileName, value);
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

        public string EditableUsername
        {
            get => _editableUsername;
            set => SetProperty(ref _editableUsername, value);
        }

        public ICommand ChangeImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public event EventHandler? SettingsSaved;
        public event EventHandler? BackRequested;
        public event EventHandler<string>? UsernameChanged;

        public PlayerSettingsViewModel(User currentUser, IUserService userService, IAvatarService avatarService)
        {
            _currentUser = currentUser;
            _userService = userService;
            _avatarService = avatarService;
            _editableUsername = currentUser.Username;
            AvatarFileName = currentUser.AvatarFileName;
            AvatarImage = _avatarService.GetAvatarImage(AvatarFileName);

            ChangeImageCommand = new RelayCommand(_ => ChangeImage());
            SaveCommand = new RelayCommand(_ => SaveSettings());
            BackCommand = new RelayCommand(_ => {
                if (!string.IsNullOrEmpty(TempAvatarFileName))
                {
                    _avatarService.DeleteTemporaryAvatar(TempAvatarFileName);
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
                    if (!string.IsNullOrEmpty(TempAvatarFileName))
                    {
                        _avatarService.DeleteTemporaryAvatar(TempAvatarFileName);
                    }
                    
                    var tempFileName = _avatarService.SaveTemporaryAvatar(openFileDialog.FileName, _currentUser.Username);
                    TempAvatarFileName = tempFileName;

                    AvatarImage = _avatarService.GetTemporaryAvatarImage(tempFileName);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error processing image: " + ex.Message;
                }
            }
        }

        private void SaveSettings()
        {
            string? finalAvatarFileName = _currentUser.AvatarFileName;
            if (!string.IsNullOrEmpty(TempAvatarFileName))
            {
                _avatarService.DeleteAvatar(_currentUser.AvatarFileName);
                finalAvatarFileName = _avatarService.CommitTemporaryAvatar(TempAvatarFileName, _currentUser.Username);
                TempAvatarFileName = null;
            }
            CurrentUser.AvatarFileName = finalAvatarFileName;

            if (_currentUser.Username != EditableUsername)
            {
                if (!_userService.UpdateUsername(_currentUser.Username, EditableUsername))
                {
                    ErrorMessage = "Username already exists or is invalid";
                    return;
                }
                CurrentUser.Username = EditableUsername;
                UsernameChanged?.Invoke(this, EditableUsername);
            }
            else
            {
                _userService.UpdateUser(CurrentUser);
            }
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}
