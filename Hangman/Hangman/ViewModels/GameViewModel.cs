using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private readonly IWordRepository _wordRepository;
        private readonly ISaveGameService _saveGameService;
        private readonly IStatisticsService _statisticsService;
        private readonly IGameTimerService _timerService;
        private readonly IUserService _userService;
        private readonly IAvatarService _avatarService;
        private readonly User _currentUser;

        private GameState _gameState;
        private string _feedbackMessage = string.Empty;
        private Brush _feedbackColor = Brushes.Transparent;
        private bool _showFeedback;
        private bool _isTransitioning;
        private readonly HashSet<string> _usedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

        public GameViewModel(
            GameState gameState,
            IWordRepository wordRepository,
            ISaveGameService saveGameService,
            IStatisticsService statisticsService,
            IGameTimerService timerService,
            IUserService userService,
            IAvatarService avatarService,
            User currentUser)
        {
            GameState = gameState;
            _wordRepository = wordRepository;
            _saveGameService = saveGameService;
            _statisticsService = statisticsService;
            _timerService = timerService;
            _userService = userService;
            _avatarService = avatarService;
            _currentUser = currentUser;
            _usedWords.Add(gameState.CurrentWord);

            NotifyAllProperties();

            LetterButtons = new ObservableCollection<LetterButtonViewModel>();
            for (char c = 'A'; c <= 'Z'; c++)
                LetterButtons.Add(new LetterButtonViewModel(c));

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

            _timerService.GameTimerTick += OnGameTimerTick;
            _timerService.StartGameTimer();
        }

        public void ProcessKeyboardLetter(char letter)
        {
            letter = char.ToUpper(letter);
            if (letter >= 'A' && letter <= 'Z' && IsGameActive)
            {
                var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
                if (btn != null && !btn.IsUsed)
                    GuessLetter(letter);
            }
        }

        private void OnGameTimerTick(object? sender, EventArgs e)
        {
            GameState.DecrementTimer();
            OnPropertyChanged(nameof(TimerDisplay));

            if (GameState.IsGameOver)
            {
                _timerService.StopGameTimer();
                HandleWordLost();
            }
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

            ShowFeedbackMessage(isCorrect ? "Correct! ✓" : "Wrong! ✗", isCorrect);

            if (GameState.IsWon)
            {
                _timerService.StopGameTimer();
                HandleWordWon();
            }
            else if (GameState.IsGameOver)
            {
                _timerService.StopGameTimer();
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
                    _saveGameService.DeleteSavedGame(GameState.SaveId.Value);

                _statisticsService.UpdateGameStatistics(
                    GameState.Username, GameState.Category, GameState.CurrentLevel, true);

                var newWord = GetNextWord(GameState.CurrentWord);

                ShowTransitionFeedback("🏆 You won 3 levels! Starting fresh...", true, () =>
                {
                    ResetRound(newWord, level: 1);
                    FocusRequested?.Invoke(this, EventArgs.Empty);
                });
            }
            else
            {
                ShowTransitionFeedback(
                    $"Level {GameState.CurrentLevel} complete! Next level...", true,
                    () => AdvanceToNextLevel());
            }
        }

        private void HandleWordLost()
        {
            _isTransitioning = true;
            OnPropertyChanged(nameof(IsGameActive));

            if (GameState.SaveId.HasValue)
                _saveGameService.DeleteSavedGame(GameState.SaveId.Value);

            _statisticsService.UpdateGameStatistics(
                GameState.Username, GameState.Category, GameState.CurrentLevel, false);

            var lostWord = GameState.CurrentWord;
            var newWord = GetNextWord(lostWord);

            ShowTransitionFeedback($"Lost! The word was: {lostWord}", false, () =>
            {
                ResetRound(newWord, level: 1);
                FocusRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private void AdvanceToNextLevel()
        {
            ResetRound(GetNextWord(GameState.CurrentWord), GameState.CurrentLevel + 1);
            FocusRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ResetRound(string newWord, int level)
        {
            GameState.CurrentWord = newWord;
            GameState.CurrentLevel = level;
            GameState.GuessedLetters = string.Empty;
            GameState.WrongGuesses = 0;
            GameState.TimeRemaining = 30;
            GameState.IsGameOver = false;
            GameState.IsWon = false;
            _usedWords.Add(newWord);

            ResetLetterButtons();
            NotifyAllProperties();
            _isTransitioning = false;

            _timerService.StartGameTimer();
        }

        private void ShowTransitionFeedback(string message, bool isPositive, Action nextAction)
        {
            FeedbackMessage = message;
            FeedbackColor = isPositive
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            ShowFeedback = true;

            _timerService.StartTransitionTimer(() =>
            {
                ShowFeedback = false;
                _isTransitioning = false;
                nextAction();
            });
        }

        private void ShowFeedbackMessage(string message, bool isCorrect)
        {
            FeedbackMessage = message;
            FeedbackColor = isCorrect
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            ShowFeedback = true;

            _timerService.StartFeedbackTimer(() => ShowFeedback = false);
        }

        private string GetNextWord(string excludeWord)
        {
            var availableWords = GameState.Category == "All Categories"
                ? _wordRepository.GetAllCategoryNames()
                    .SelectMany(cat => _wordRepository.GetWordsForCategory(cat))
                    .Where(w => !_usedWords.Contains(w))
                    .ToList()
                : _wordRepository.GetWordsForCategory(GameState.Category)
                    .Where(w => !_usedWords.Contains(w))
                    .ToList();

            if (availableWords.Count == 0)
            {
                _usedWords.Clear();
                return GameState.Category == "All Categories"
                    ? _wordRepository.GetRandomWordFromAllCategories()
                    : _wordRepository.GetRandomWord(GameState.Category);
            }

            var random = new Random();
            var newWord = availableWords[random.Next(availableWords.Count)];
            _usedWords.Add(newWord);
            return newWord;
        }

        private void ResetLetterButtons()
        {
            foreach (var btn in LetterButtons)
                btn.IsUsed = false;
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

        public void SaveGame()
        {
            string saveName = $"Save {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var saveId = _saveGameService.SaveGame(GameState, saveName);
            GameState.SaveId = saveId;
            MessageBox.Show($"Game saved successfully as '{saveName}'!", "Save Game",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitGame()
        {
            _timerService.StopGameTimer();
            GameExitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void StopTimer() => _timerService.StopGameTimer();
    }
}
