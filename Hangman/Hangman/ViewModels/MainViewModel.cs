using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Hangman.Models;
using Hangman.Services;
using Hangman.Views;

namespace Hangman.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IWordRepository _wordRepository;
        private readonly IGameFactory _gameFactory;
        private readonly ISaveGameService _saveGameService;
        private readonly IStatisticsService _statisticsService;
        private readonly IAvatarService _avatarService;
        private readonly IDialogService _dialogService;
        private readonly IGameTimerServiceFactory _timerFactory;

        private object? _currentView;
        private User? _currentUser;
        private bool _isMenuVisible;
        private string _selectedCategory = "All Categories";
        private ObservableCollection<string> _categories;
        private GameViewModel? _currentGameViewModel;

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set => SetProperty(ref _isMenuVisible, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    OnPropertyChanged(nameof(SelectedCategoryDisplay));
            }
        }

        public string SelectedCategoryDisplay => SelectedCategory;

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand OpenGameCommand { get; private set; }
        public RelayCommand SaveCurrentGameCommand { get; private set; }
        public RelayCommand ShowStatisticsCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand SelectCategoryCommand { get; private set; }
        public RelayCommand AboutCommand { get; private set; }

        public MainViewModel()
        {
            var statisticsService = new StatisticsService();
            var wordRepo = new WordRepository();
            var saveGameService = new SaveGameService(wordRepository: wordRepo);
            var userService = new UserService(statisticsService: statisticsService, saveGameService: saveGameService);
            var gameFactory = new GameFactory(wordRepo);
            
            _userService = userService;
            _wordRepository = wordRepo;
            _gameFactory = gameFactory;
            _saveGameService = saveGameService;
            _statisticsService = statisticsService;
            _avatarService = new AvatarService();
            _dialogService = new WpfDialogService(userService);
            _timerFactory = new GameTimerServiceFactory();

            _categories = new ObservableCollection<string>(new[] { "All Categories" }.Concat(_wordRepository.GetAllCategoryNames()));

            InitializeCommands();
            ShowUserSelection();
        }

        public MainViewModel(
            IUserService userService,
            IWordRepository wordRepository,
            IGameFactory gameFactory,
            ISaveGameService saveGameService,
            IStatisticsService statisticsService,
            IAvatarService avatarService,
            IDialogService? dialogService = null,
            IGameTimerServiceFactory? timerFactory = null)
        {
            _userService = userService;
            _wordRepository = wordRepository;
            _gameFactory = gameFactory;
            _saveGameService = saveGameService;
            _statisticsService = statisticsService;
            _avatarService = avatarService;
            _dialogService = dialogService ?? new WpfDialogService(userService);
            _timerFactory = timerFactory ?? new GameTimerServiceFactory();

            _categories = new ObservableCollection<string>(new[] { "All Categories" }.Concat(_wordRepository.GetAllCategoryNames()));

            InitializeCommands();
            ShowUserSelection();
        }

        private void InitializeCommands()
        {
            NewGameCommand = new RelayCommand(
                _ => StartNewGame(SelectedCategory), _ => IsMenuVisible);
            OpenGameCommand = new RelayCommand(
                _ => ContinueSavedGame(), _ => IsMenuVisible);
            SaveCurrentGameCommand = new RelayCommand(
                _ => SaveCurrentGame(), _ => _currentGameViewModel != null);
            ShowStatisticsCommand = new RelayCommand(
                _ => ShowStatistics(), _ => IsMenuVisible);
            CancelCommand = new RelayCommand(
                _ => Logout(), _ => IsMenuVisible);
            SelectCategoryCommand = new RelayCommand(
                param => SelectCategory(param));
            AboutCommand = new RelayCommand(
                _ => ShowAbout());
        }

        private void SetView(object view, object viewModel)
        {
            ((System.Windows.FrameworkElement)view).DataContext = viewModel;
            CurrentView = view;
        }

        private void ShowUserSelection()
        {
            IsMenuVisible = false;
            _currentGameViewModel = null;

            var view = new UserSelectionView();
            var vm = new UserSelectionViewModel(_userService, _saveGameService, _statisticsService, _avatarService);

            vm.NewUserRequested  += (_, _) => ShowSignUp();
            vm.PlayRequested     += (_, user) => ShowPasswordDialog(user);
            vm.CancelRequested   += (_, _) => Application.Current.Shutdown();

            SetView(view, vm);
        }

        private void ShowSignUp()
        {
            var view = new SignUpView();
            var vm   = new SignUpViewModel(_userService, _avatarService);

            vm.SignUpSuccessful      += (_, _) => ShowUserSelection();
            vm.BackToLoginRequested  += (_, _) => ShowUserSelection();

            SetView(view, vm);
        }

        private void ShowMainMenu()
        {
            _currentGameViewModel = null;

            var view = new MainMenuView();
            var vm = new MainMenuViewModel(
                CurrentUser!, _userService,
                _saveGameService, _statisticsService, _avatarService);

            vm.LogoutRequested        += (_, _) => Logout();
            vm.NewGameRequested       += (_, _) => StartNewGame(SelectedCategory);
            vm.ContinueGameRequested  += (_, _) => ContinueSavedGame();
            vm.SettingsRequested      += (_, _) => ShowSettingsHub();
            vm.StatisticsRequested    += (_, _) => ShowStatistics();

            SetView(view, vm);
        }

        private void ShowStatistics()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;

            var view = new StatisticsView();
            var vm   = new StatisticsViewModel(_statisticsService);
            vm.BackRequested += (_, _) => ShowMainMenu();

            SetView(view, vm);
        }

        private void ShowGame(GameState gameState)
        {
            var view = new GameView();
            var vm   = new GameViewModel(
                gameState,
                _wordRepository,
                _saveGameService,
                _statisticsService,
                _timerFactory.Create(),
                _userService,
                _avatarService,
                CurrentUser!);

            vm.GameExitRequested += (_, _) => ShowMainMenu();

            _currentGameViewModel = vm;
            SetView(view, vm);
        }

        private void ShowSettingsHub()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;

            var view = new SettingsHubView();
            var vm = new SettingsHubViewModel(CurrentUser!, _avatarService);

            vm.EditProfileRequested += (_, _) => ShowPlayerSettings();
            vm.EditWordRepositoryRequested += (_, _) => ShowWordRepository();
            vm.BackRequested += (_, _) => ShowMainMenu();

            SetView(view, vm);
        }

        private void ShowPlayerSettings()
        {
            var view = new PlayerSettingsView();
            var vm   = new PlayerSettingsViewModel(CurrentUser!, _userService, _avatarService);

            vm.SettingsSaved += (_, _) =>
            {
                ShowSettingsHub();
            };
            vm.UsernameChanged += (_, newUsername) =>
            {
                CurrentUser = _userService.GetUser(newUsername);
            };
            vm.BackRequested += (_, _) => ShowSettingsHub();

            SetView(view, vm);
        }

        private void ShowWordRepository()
        {
            var view = new CategoriesManagementView();
            var vm = new CategoriesManagementViewModel(_wordRepository);

            vm.BackRequested += (_, _) => ShowSettingsHub();

            SetView(view, vm);
        }

        private void SelectCategory(object? param)
        {
            if (param is string categoryStr)
            {
                SelectedCategory = categoryStr;
                if (_currentGameViewModel != null)
                    StartNewGame(categoryStr);
            }
        }

        private void StartNewGame(string category)
        {
            _currentGameViewModel?.StopTimer();
            var gameState = _gameFactory.CreateNewGame(CurrentUser!.Username, category);
            ShowGame(gameState);
        }

        private void SaveCurrentGame() => _currentGameViewModel?.SaveGame();

        private void Logout()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;
            IsMenuVisible = false;
            CurrentUser = null;
            ShowUserSelection();
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Hangman\n\n" +
                "Nume student: Șomu George-Sebastian\n" +
                "Grupa: 10LF244\n" +
                "Specializarea: Informatica\n\n",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowPasswordDialog(User user)
        {
            if (_dialogService.ShowPasswordDialog(user))
            {
                CurrentUser = user;
                IsMenuVisible = true;
                ShowMainMenu();
            }
        }

        private void ContinueSavedGame()
        {
            var savedGames = _saveGameService.GetSavedGamesForUser(CurrentUser!.Username);

            if (savedGames.Count == 0)
            {
                MessageBox.Show("No saved games found.", "Open Game",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedSave = _dialogService.ShowSaveGameSelection(
                savedGames,
                id => _saveGameService.DeleteSavedGame(id));

            if (selectedSave != null)
            {
                var gameState = _gameFactory.LoadGameFromSave(selectedSave);
                if (gameState != null)
                    ShowGame(gameState);
            }
        }
    }
}
