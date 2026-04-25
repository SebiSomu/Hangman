using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class SettingsHubViewModel : ViewModelBase
    {
        private readonly IAvatarService _avatarService;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string WelcomeMessage => $"Settings for {CurrentUser.Username}";
        public BitmapImage? AvatarImage => _avatarService.GetAvatarImage(CurrentUser.AvatarFileName);

        public ICommand EditProfileCommand { get; }
        public ICommand EditWordRepositoryCommand { get; }
        public ICommand BackCommand { get; }

        public event EventHandler? EditProfileRequested;
        public event EventHandler? EditWordRepositoryRequested;
        public event EventHandler? BackRequested;

        public SettingsHubViewModel(User currentUser, IAvatarService avatarService)
        {
            _currentUser = currentUser;
            _avatarService = avatarService;

            EditProfileCommand = new RelayCommand(_ => EditProfileRequested?.Invoke(this, EventArgs.Empty));
            EditWordRepositoryCommand = new RelayCommand(_ => EditWordRepositoryRequested?.Invoke(this, EventArgs.Empty));
            BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));
        }
    }
}
