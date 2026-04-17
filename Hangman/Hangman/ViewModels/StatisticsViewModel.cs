using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class UserStatisticsItem : ViewModelBase
    {
        public string Username { get; set; } = string.Empty;
        public int TotalGamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public double WinRate { get; set; }
        public string WinRateDisplay => $"{WinRate:F1}%";
        public List<CategoryStats> CategoryStatistics { get; set; } = new();
    }

    public class StatisticsViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;

        public List<UserStatisticsItem> AllUserStats { get; private set; } = new();
        public bool HasData => AllUserStats.Count > 0;

        public RelayCommand BackCommand { get; }

        public event EventHandler? BackRequested;

        public StatisticsViewModel(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            LoadAllStatistics();
            BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));
        }

        private void LoadAllStatistics()
        {
            AllUserStats = new List<UserStatisticsItem>();

            var allStats = _statisticsService.GetAllStatistics();

            foreach (var kvp in allStats)
            {
                var userStats = kvp.Value;
                AllUserStats.Add(new UserStatisticsItem
                {
                    Username = kvp.Key,
                    TotalGamesPlayed = userStats.TotalGamesPlayed,
                    GamesWon = userStats.GamesWon,
                    GamesLost = userStats.GamesLost,
                    WinRate = userStats.TotalGamesPlayed > 0
                        ? (double)userStats.GamesWon / userStats.TotalGamesPlayed * 100
                        : 0,
                    CategoryStatistics = userStats.CategoryStats ?? new List<CategoryStats>()
                });
            }

            OnPropertyChanged(nameof(AllUserStats));
            OnPropertyChanged(nameof(HasData));
        }
    }
}
