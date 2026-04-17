using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Hangman.Models;
using Hangman.Services;
using Hangman.Views;

namespace Hangman
{
    public class CategoryFontWeightHelper : IValueConverter
    {
        public static readonly CategoryFontWeightHelper Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selected && parameter is string param)
            {
                return selected == param ? FontWeights.Bold : FontWeights.Normal;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Hangman.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly GameService _gameService;
        private readonly AvatarService _avatarService;
        private object? _currentView;
        private User? _currentUser;
        private bool _isMenuVisible;
        private WordCategory _selectedCategory = WordCategory.AllCategories;
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

        public WordCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    OnPropertyChanged(nameof(SelectedCategoryDisplay));
                }
            }
        }

        public string SelectedCategoryDisplay => SelectedCategory.GetDisplayName();

        public RelayCommand NewGameCommand { get; }
        public RelayCommand OpenGameCommand { get; }
        public RelayCommand SaveCurrentGameCommand { get; }
        public RelayCommand ShowStatisticsCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SelectCategoryCommand { get; }
        public RelayCommand AboutCommand { get; }

        public MainViewModel()
        {
            _userService = new UserService();
            _gameService = new GameService();
            _avatarService = new AvatarService();

            NewGameCommand = new RelayCommand(_ => StartNewGame(SelectedCategory), _ => IsMenuVisible);
            OpenGameCommand = new RelayCommand(_ => ContinueSavedGame(), _ => IsMenuVisible);
            SaveCurrentGameCommand = new RelayCommand(_ => SaveCurrentGame(), _ => _currentGameViewModel != null);
            ShowStatisticsCommand = new RelayCommand(_ => ShowStatistics(), _ => IsMenuVisible);
            CancelCommand = new RelayCommand(_ => Logout(), _ => IsMenuVisible);
            SelectCategoryCommand = new RelayCommand(param => SelectCategory(param));
            AboutCommand = new RelayCommand(_ => ShowAbout());

            ShowUserSelection();
        }

        private void SelectCategory(object? param)
        {
            if (param is string categoryStr)
            {
                if (Enum.TryParse<WordCategory>(categoryStr, out var category))
                {
                    SelectedCategory = category;
                    if (_currentGameViewModel != null)
                    {
                        StartNewGame(category);
                    }
                }
            }
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

        private void SaveCurrentGame()
        {
            _currentGameViewModel?.SaveGame();
        }

        private void Logout()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;
            IsMenuVisible = false;
            CurrentUser = null;
            ShowUserSelection();
        }

        private void ShowUserSelection()
        {
            IsMenuVisible = false;
            _currentGameViewModel = null;

            var userSelectionView = new UserSelectionView();
            var userSelectionViewModel = new UserSelectionViewModel(_userService);

            userSelectionViewModel.NewUserRequested += (sender, e) => ShowSignUp();
            userSelectionViewModel.PlayRequested += (sender, user) => ShowPasswordDialog(user);
            userSelectionViewModel.CancelRequested += (sender, e) => Application.Current.Shutdown();

            userSelectionView.DataContext = userSelectionViewModel;
            CurrentView = userSelectionView;
        }

        private void ShowSignUp()
        {
            var signUpView = new SignUpView();
            var signUpViewModel = new SignUpViewModel(_userService, _avatarService);
            signUpViewModel.SignUpSuccessful += (sender, e) => ShowUserSelection();
            signUpViewModel.BackToLoginRequested += (sender, e) => ShowUserSelection();
            signUpView.DataContext = signUpViewModel;
            CurrentView = signUpView;
        }

        private void ShowPasswordDialog(User user)
        {
            var passwordWindow = new Window
            {
                Title = "Enter Password",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = $"Enter password for {user.Username}:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var passwordBox = new PasswordBox
            {
                Height = 35,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(passwordBox);

            var errorText = new TextBlock
            {
                Foreground = System.Windows.Media.Brushes.Red,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 10),
                Visibility = Visibility.Collapsed
            };
            stackPanel.Children.Add(errorText);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 102, 241)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };
            okButton.Click += (s, e) =>
            {
                if (_userService.ValidateUser(user.Username, passwordBox.Password))
                {
                    passwordWindow.DialogResult = true;
                    passwordWindow.Close();
                }
                else
                {
                    errorText.Text = "Incorrect password.";
                    errorText.Visibility = Visibility.Visible;
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 32
            };
            cancelButton.Click += (s, e) =>
            {
                passwordWindow.DialogResult = false;
                passwordWindow.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            passwordWindow.Content = stackPanel;

            if (passwordWindow.ShowDialog() == true)
            {
                CurrentUser = user;
                IsMenuVisible = true;
                ShowMainMenu();
            }
        }

        private void ShowMainMenu()
        {
            _currentGameViewModel = null;

            var mainMenuView = new MainMenuView();
            var mainMenuViewModel = new MainMenuViewModel(CurrentUser!, _userService, _gameService, _avatarService);
            mainMenuViewModel.LogoutRequested += (sender, e) => Logout();
            mainMenuViewModel.NewGameRequested += (sender, e) => StartNewGame(SelectedCategory);
            mainMenuViewModel.ContinueGameRequested += (sender, e) => ContinueSavedGame();
            mainMenuViewModel.PlayerSettingsRequested += (sender, e) => ShowPlayerSettings();
            mainMenuViewModel.StatisticsRequested += (sender, e) => ShowStatistics();
            mainMenuView.DataContext = mainMenuViewModel;
            CurrentView = mainMenuView;
        }

        private void ShowStatistics()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;

            var statsView = new StatisticsView();
            var statsViewModel = new StatisticsViewModel(_gameService);
            statsViewModel.BackRequested += (sender, e) => ShowMainMenu();
            statsView.DataContext = statsViewModel;
            CurrentView = statsView;
        }

        private void StartNewGame(WordCategory category)
        {
            _currentGameViewModel?.StopTimer();

            var gameState = _gameService.CreateNewGame(CurrentUser!.Username, category);
            ShowGame(gameState);
        }

        private void ContinueSavedGame()
        {
            var savedGames = _gameService.GetSavedGamesForUser(CurrentUser!.Username);

            if (savedGames.Count == 0)
            {
                MessageBox.Show("No saved games found.", "Open Game",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectWindow = new Window
            {
                Title = "Select Saved Game",
                Width = 450,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Select a saved game to continue:",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15)
            });

            GameSaveData? selectedSave = null;
            var listBox = new ListBox
            {
                Height = 220,
                Margin = new Thickness(0, 0, 0, 15)
            };

            foreach (var save in savedGames)
            {
                var item = new StackPanel { Margin = new Thickness(5) };
                item.Children.Add(new TextBlock
                {
                    Text = save.SaveName,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                });
                item.Children.Add(new TextBlock
                {
                    Text = $"Category: {save.Category} | Level: {save.CurrentLevel} | Saved: {save.SavedAt:g}",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 12
                });
                item.Tag = save;
                listBox.Items.Add(item);
            }

            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;

            stackPanel.Children.Add(listBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var loadButton = new Button
            {
                Content = "Load",
                Width = 80,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 102, 241)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };
            loadButton.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    selectedSave = (GameSaveData)((StackPanel)listBox.SelectedItem).Tag;
                    selectWindow.DialogResult = true;
                    selectWindow.Close();
                }
            };

            var deleteButton = new Button
            {
                Content = "Delete",
                Width = 80,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };
            deleteButton.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    var save = (GameSaveData)((StackPanel)listBox.SelectedItem).Tag;
                    var result = MessageBox.Show($"Delete save '{save.SaveName}'?", "Confirm Delete",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _gameService.DeleteSavedGame(save.SaveId);
                        listBox.Items.Remove(listBox.SelectedItem);
                        if (listBox.Items.Count == 0)
                        {
                            selectWindow.Close();
                        }
                    }
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 32,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 231, 235))
            };
            cancelButton.Click += (s, e) =>
            {
                selectWindow.DialogResult = false;
                selectWindow.Close();
            };

            buttonPanel.Children.Add(loadButton);
            buttonPanel.Children.Add(deleteButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            selectWindow.Content = stackPanel;

            if (selectWindow.ShowDialog() == true && selectedSave != null)
            {
                var gameState = _gameService.LoadGameFromSave(selectedSave);
                if (gameState != null)
                {
                    ShowGame(gameState);
                }
            }
        }

        private void ShowGame(GameState gameState)
        {
            var gameView = new GameView();
            var gameViewModel = new GameViewModel(gameState, _gameService, _userService, _avatarService, CurrentUser!);

            gameViewModel.GameExitRequested += (sender, e) => ShowMainMenu();

            _currentGameViewModel = gameViewModel;
            gameView.DataContext = gameViewModel;
            CurrentView = gameView;
        }

        private void ShowPlayerSettings()
        {
            _currentGameViewModel?.StopTimer();
            _currentGameViewModel = null;

            var playerSettingsView = new PlayerSettingsView();
            var playerSettingsViewModel = new PlayerSettingsViewModel(CurrentUser!, _userService, _avatarService);

            playerSettingsViewModel.SettingsSaved += (sender, e) =>
            {
                CurrentUser = _userService.GetUser(CurrentUser!.Username);
                ShowMainMenu();
            };
            playerSettingsViewModel.BackRequested += (sender, e) => ShowMainMenu();

            playerSettingsView.DataContext = playerSettingsViewModel;
            CurrentView = playerSettingsView;
        }
    }
}
