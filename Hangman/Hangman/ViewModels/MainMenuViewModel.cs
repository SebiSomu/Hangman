using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly ISaveGameService _saveGameService;
        private readonly IStatisticsService _statisticsService;
        private readonly IAvatarService _avatarService;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string WelcomeMessage => $"Welcome, {CurrentUser.Username}!";
        public string? AvatarFileName => CurrentUser.AvatarFileName;
        public BitmapImage? AvatarImage => _avatarService.GetAvatarImage(CurrentUser.AvatarFileName);

        public ICommand NewGameCommand { get; }
        public ICommand ContinueGameCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand PlayerSettingsCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand LogoutCommand { get; }

        public event EventHandler? LogoutRequested;
        public event EventHandler? NewGameRequested;
        public event EventHandler? ContinueGameRequested;
        public event EventHandler? StatisticsRequested;
        public event EventHandler? PlayerSettingsRequested;

        public MainMenuViewModel(
            User currentUser,
            IUserService userService,
            ISaveGameService saveGameService,
            IStatisticsService statisticsService,
            IAvatarService avatarService)
        {
            _currentUser = currentUser;
            _userService = userService;
            _saveGameService = saveGameService;
            _statisticsService = statisticsService;
            _avatarService = avatarService;

            NewGameCommand = new RelayCommand(_ => NewGameRequested?.Invoke(this, EventArgs.Empty));
            ContinueGameCommand = new RelayCommand(_ => ContinueGameRequested?.Invoke(this, EventArgs.Empty));
            StatisticsCommand = new RelayCommand(_ => StatisticsRequested?.Invoke(this, EventArgs.Empty));
            PlayerSettingsCommand = new RelayCommand(_ => PlayerSettingsRequested?.Invoke(this, EventArgs.Empty));
            DeleteAccountCommand = new RelayCommand(_ => DeleteAccount());
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke(this, EventArgs.Empty));
        }

        private void DeleteAccount()
        {
            var result = System.Windows.MessageBox.Show(
                "Are you sure you want to delete your account? All data including saved games and statistics will be lost!",
                "Confirm deletion",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _avatarService.DeleteAvatar(CurrentUser.AvatarFileName);
                _saveGameService.DeleteAllSavedGamesForUser(CurrentUser.Username);
                _statisticsService.DeleteUserStatistics(CurrentUser.Username);
                _userService.DeleteUser(CurrentUser.Username);
                System.Windows.MessageBox.Show("Account deleted successfully!");
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
