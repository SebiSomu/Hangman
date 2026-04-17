using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class LetterButtonViewModel : ViewModelBase
    {
        private bool _isUsed;

        public char Letter { get; }

        public bool IsUsed
        {
            get => _isUsed;
            set => SetProperty(ref _isUsed, value);
        }

        public LetterButtonViewModel(char letter)
        {
            Letter = letter;
        }
    }

    public class GameViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        private readonly UserService _userService;
        private readonly AvatarService _avatarService;
        private readonly User _currentUser;
        private GameState _gameState;
        private DispatcherTimer? _timer;
        private DispatcherTimer? _transitionTimer;
        private DispatcherTimer? _feedbackTimer;
        private string _feedbackMessage = string.Empty;
        private Brush _feedbackColor = Brushes.Transparent;
        private bool _showFeedback;
        private bool _isTransitioning;

        public GameState GameState
        {
            get => _gameState;
            set => SetProperty(ref _gameState, value);
        }

        public string MaskedWord => GameState.GetMaskedWord();

        public ObservableCollection<LetterButtonViewModel> LetterButtons { get; }

        public string PlayerName => _currentUser.Username;
        public string? PlayerAvatarFileName => _currentUser.AvatarFileName;
        public BitmapImage? PlayerAvatarImage => _avatarService.GetAvatarImage(_currentUser.AvatarFileName);

        public string FeedbackMessage
        {
            get => _feedbackMessage;
            set => SetProperty(ref _feedbackMessage, value);
        }

        public Brush FeedbackColor
        {
            get => _feedbackColor;
            set => SetProperty(ref _feedbackColor, value);
        }

        public bool ShowFeedback
        {
            get => _showFeedback;
            set => SetProperty(ref _showFeedback, value);
        }

        public string TimerDisplay => $"Time: {GameState.TimeRemaining}s";
        public string LevelDisplay => $"Level: {GameState.CurrentLevel}/3";
        public string CategoryDisplay => $"Category: {GameState.Category}";

        public bool IsGameActive => !GameState.IsGameOver && !_isTransitioning;

        public ICommand LetterClickCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand ExitGameCommand { get; }

        public event EventHandler? GameExitRequested;
        public event EventHandler? FocusRequested;

        public GameViewModel(GameState gameState, GameService gameService, UserService userService, AvatarService avatarService, User currentUser)
        {
            _gameState = gameState;
            _gameService = gameService;
            _userService = userService;
            _avatarService = avatarService;
            _currentUser = currentUser;

            LetterButtons = new ObservableCollection<LetterButtonViewModel>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                LetterButtons.Add(new LetterButtonViewModel(c));
            }

            if (!string.IsNullOrEmpty(gameState.GuessedLetters))
            {
                foreach (char c in gameState.GuessedLetters)
                {
                    var btn = LetterButtons.FirstOrDefault(b => b.Letter == char.ToUpper(c));
                    if (btn != null) btn.IsUsed = true;
                }
            }

            LetterClickCommand = new RelayCommand(
                param =>
                {
                    char letter;
                    if (param is char ch) letter = ch;
                    else if (param is string s && s.Length == 1) letter = s[0];
                    else return;
                    GuessLetter(letter);
                },
                param => IsGameActive
            );
            SaveGameCommand = new RelayCommand(_ => SaveGame(), _ => IsGameActive);
            ExitGameCommand = new RelayCommand(_ => ExitGame());

            StartTimer();
        }

        public void ProcessKeyboardLetter(char letter)
        {
            letter = char.ToUpper(letter);
            if (letter >= 'A' && letter <= 'Z' && IsGameActive)
            {
                var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
                if (btn != null && !btn.IsUsed)
                {
                    GuessLetter(letter);
                }
            }
        }

        private void StartTimer()
        {
            _timer?.Stop();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (sender, e) =>
            {
                GameState.DecrementTimer();
                OnPropertyChanged(nameof(TimerDisplay));

                if (GameState.IsGameOver)
                {
                    _timer?.Stop();
                    HandleWordLost();
                }
            };
            _timer.Start();
        }

        private void GuessLetter(char letter)
        {
            letter = char.ToUpper(letter);

            var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
            if (btn == null || btn.IsUsed) return;
            btn.IsUsed = true;

            if (GameState.IsLetterGuessed(letter)) return;

            var isCorrect = GameState.GuessLetter(letter);

            GameState.TimeRemaining = 30;
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(GameState));
            OnPropertyChanged(nameof(MaskedWord));

            if (isCorrect)
            {
                ShowFeedbackMessage("Correct! ✓", true);
            }
            else
            {
                ShowFeedbackMessage("Wrong! ✗", false);
            }

            if (GameState.IsWon)
            {
                _timer?.Stop();
                HandleWordWon();
            }
            else if (GameState.IsGameOver)
            {
                _timer?.Stop();
                HandleWordLost();
            }
        }

        private void HandleWordWon()
        {
            _isTransitioning = true;
            OnPropertyChanged(nameof(IsGameActive));

            if (GameState.CurrentLevel >= 3)
            {
                if (GameState.SaveId.HasValue)
                {
                    _gameService.DeleteSavedGame(GameState.SaveId.Value);
                }

                _gameService.UpdateGameStatistics(GameState.Username, GameState.Category, GameState.CurrentLevel, true);

                var newWord = GetNextWord(GameState.CurrentWord);

                ShowTransitionFeedback("🏆 You won 3 levels! Starting fresh...", true, () =>
                {
                    GameState.CurrentWord = newWord;
                    GameState.CurrentLevel = 1;
                    GameState.GuessedLetters = string.Empty;
                    GameState.WrongGuesses = 0;
                    GameState.TimeRemaining = 30;
                    GameState.IsGameOver = false;
                    GameState.IsWon = false;

                    ResetLetterButtons();
                    NotifyAllProperties();
                    StartTimer();
                    FocusRequested?.Invoke(this, EventArgs.Empty);
                });
            }
            else
            {
                ShowTransitionFeedback($"Level {GameState.CurrentLevel} complete! Next level...", true, () => AdvanceToNextLevel());
            }
        }

        private void HandleWordLost()
        {
            _isTransitioning = true;
            OnPropertyChanged(nameof(IsGameActive));

            if (GameState.SaveId.HasValue)
            {
                _gameService.DeleteSavedGame(GameState.SaveId.Value);
            }

            _gameService.UpdateGameStatistics(GameState.Username, GameState.Category, GameState.CurrentLevel, false);
            string lostWord = GameState.CurrentWord;

            var newWord = GetNextWord(GameState.CurrentWord);

            ShowTransitionFeedback($"Lost! The word was: {lostWord}", false, () =>
            {
                GameState.CurrentWord = newWord;
                GameState.CurrentLevel = 1;
                GameState.GuessedLetters = string.Empty;
                GameState.WrongGuesses = 0;
                GameState.TimeRemaining = 30;
                GameState.IsGameOver = false;
                GameState.IsWon = false;

                ResetLetterButtons();
                NotifyAllProperties();
                StartTimer();
                FocusRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private void ShowTransitionFeedback(string message, bool isPositive, Action nextAction)
        {
            FeedbackMessage = message;
            FeedbackColor = isPositive
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            ShowFeedback = true;

            _feedbackTimer?.Stop();
            _transitionTimer?.Stop();
            _transitionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _transitionTimer.Tick += (s, e) =>
            {
                _transitionTimer.Stop();
                ShowFeedback = false;
                _isTransitioning = false;
                nextAction();
            };
            _transitionTimer.Start();
        }

        private void AdvanceToNextLevel()
        {
            var newWord = GetNextWord(GameState.CurrentWord);

            GameState.CurrentWord = newWord;
            GameState.CurrentLevel++;
            GameState.GuessedLetters = string.Empty;
            GameState.WrongGuesses = 0;
            GameState.TimeRemaining = 30;
            GameState.IsGameOver = false;
            GameState.IsWon = false;

            ResetLetterButtons();
            NotifyAllProperties();
            StartTimer();
            FocusRequested?.Invoke(this, EventArgs.Empty);
        }

        private void StartFreshRound()
        {
            var newWord = GetNextWord(GameState.CurrentWord);

            GameState.CurrentWord = newWord;
            GameState.CurrentLevel = 1;
            GameState.GuessedLetters = string.Empty;
            GameState.WrongGuesses = 0;
            GameState.TimeRemaining = 30;
            GameState.IsGameOver = false;
            GameState.IsWon = false;

            ResetLetterButtons();
            NotifyAllProperties();
            StartTimer();
            FocusRequested?.Invoke(this, EventArgs.Empty);
        }

        private string GetNextWord(string excludeWord)
        {
            if (GameState.Category == "All Categories")
                return _gameService.GetRandomWordFromAllCategoriesExcluding(excludeWord);
            else
                return _gameService.GetRandomWordExcluding(GameState.Category, excludeWord);
        }

        private void ResetLetterButtons()
        {
            foreach (var btn in LetterButtons)
            {
                btn.IsUsed = false;
            }
        }

        private void NotifyAllProperties()
        {
            OnPropertyChanged(nameof(MaskedWord));
            OnPropertyChanged(nameof(LevelDisplay));
            OnPropertyChanged(nameof(CategoryDisplay));
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(IsGameActive));
            OnPropertyChanged(nameof(GameState));
        }

        private void ShowFeedbackMessage(string message, bool isCorrect)
        {
            FeedbackMessage = message;
            FeedbackColor = isCorrect
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            ShowFeedback = true;

            _feedbackTimer?.Stop();
            _feedbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };
            _feedbackTimer.Tick += (s, e) =>
            {
                ShowFeedback = false;
                _feedbackTimer.Stop();
            };
            _feedbackTimer.Start();
        }

        public void SaveGame()
        {
            string saveName = $"Save {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            _gameService.SaveGame(GameState, saveName);
            MessageBox.Show($"Game saved successfully as '{saveName}'!", "Save Game",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitGame()
        {
            _timer?.Stop();
            GameExitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }
    }
}
