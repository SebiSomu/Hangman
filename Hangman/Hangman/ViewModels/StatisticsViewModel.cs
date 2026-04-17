 using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        public List<GameService.CategoryStats> CategoryStatistics { get; set; } = new();
    }

    public class StatisticsViewModel : ViewModelBase
    {
        private readonly GameService _gameService;

        public List<UserStatisticsItem> AllUserStats { get; private set; } = new();
        public bool HasData => AllUserStats.Count > 0;

        public RelayCommand BackCommand { get; }

        public event EventHandler? BackRequested;

        public StatisticsViewModel(GameService gameService)
        {
            _gameService = gameService;
            LoadAllStatistics();
            BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));
        }

        private void LoadAllStatistics()
        {
            AllUserStats = new List<UserStatisticsItem>();

            var statsFile = "gamestats.json";

            if (!File.Exists(statsFile))
            {
                statsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gamestats.json");
            }

            if (File.Exists(statsFile))
            {
                var json = File.ReadAllText(statsFile);
                var allStats = JsonSerializer.Deserialize<Dictionary<string, GameService.UserStats>>(json);

                if (allStats != null)
                {
                    foreach (var kvp in allStats)
                    {
                        var userStats = kvp.Value;
                        var item = new UserStatisticsItem
                        {
                            Username = kvp.Key,
                            TotalGamesPlayed = userStats.TotalGamesPlayed,
                            GamesWon = userStats.GamesWon,
                            GamesLost = userStats.GamesLost,
                            WinRate = userStats.TotalGamesPlayed > 0
                                ? (double)userStats.GamesWon / userStats.TotalGamesPlayed * 100
                                : 0,
                            CategoryStatistics = userStats.CategoryStats ?? new List<GameService.CategoryStats>()
                        };
                        AllUserStats.Add(item);
                    }
                }
            }

            OnPropertyChanged(nameof(AllUserStats));
            OnPropertyChanged(nameof(HasData));
        }
    }
}
