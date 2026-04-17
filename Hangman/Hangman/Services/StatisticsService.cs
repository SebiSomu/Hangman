using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly string _statsFile;

        public StatisticsService(string statsFile = "gamestats.json")
        {
            _statsFile = statsFile;
        }

        public void UpdateGameStatistics(string username, string category, int level, bool isWon)
        {
            var allStats = LoadAll();

            if (!allStats.TryGetValue(username, out var userStats))
            {
                userStats = new UserStats();
                allStats[username] = userStats;
            }

            userStats.TotalGamesPlayed++;
            if (isWon) userStats.GamesWon++;
            else userStats.GamesLost++;

            var catStat = userStats.CategoryStats
                .FirstOrDefault(cs => cs.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (catStat == null)
            {
                catStat = new CategoryStats { Category = category };
                userStats.CategoryStats.Add(catStat);
            }

            catStat.GamesPlayed++;
            if (isWon) catStat.GamesWon++;
            if (level > catStat.BestLevel) catStat.BestLevel = level;

            Persist(allStats);
        }

        public void DeleteUserStatistics(string username)
        {
            if (!File.Exists(_statsFile)) return;
            var allStats = LoadAll();
            if (allStats.Remove(username))
            {
                Persist(allStats);
            }
        }

        public Dictionary<string, UserStats> GetAllStatistics() => LoadAll();

        // ── Helpers ─────────────────────────────────────────────────────────

        private Dictionary<string, UserStats> LoadAll()
        {
            var path = File.Exists(_statsFile)
                ? _statsFile
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _statsFile);

            if (!File.Exists(path)) return new Dictionary<string, UserStats>();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, UserStats>>(json)
                   ?? new Dictionary<string, UserStats>();
        }

        private void Persist(Dictionary<string, UserStats> data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_statsFile, json);
        }
    }
}
