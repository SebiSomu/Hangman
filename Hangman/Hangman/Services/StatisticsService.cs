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

            var existingKey = allStats.Keys.FirstOrDefault(k => 
                k.Equals(username, StringComparison.OrdinalIgnoreCase));
            
            if (existingKey == null || !allStats.TryGetValue(existingKey, out var userStats))
            {
                userStats = new UserStats();
                allStats[username] = userStats;
            }
            else
            {
                allStats.Remove(existingKey);
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
            var path = GetStatsFilePath();
            if (!File.Exists(path)) return;
            var allStats = LoadAll();
            
            var existingKey = allStats.Keys.FirstOrDefault(k => 
                k.Equals(username, StringComparison.OrdinalIgnoreCase));
            
            if (existingKey != null && allStats.Remove(existingKey))
            {
                Persist(allStats);
            }
        }

        public void RenameUsernameStatistics(string oldUsername, string newUsername)
        {
            var allStats = LoadAll();
            
            var existingKey = allStats.Keys.FirstOrDefault(k => 
                k.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));
            
            if (existingKey != null && allStats.TryGetValue(existingKey, out var userStats))
            {
                allStats.Remove(existingKey);
                allStats[newUsername] = userStats;
                Persist(allStats);
            }
        }

        public Dictionary<string, UserStats> GetAllStatistics() => LoadAll();

        private Dictionary<string, UserStats> LoadAll()
        {
            var path = GetStatsFilePath();

            if (!File.Exists(path)) return new Dictionary<string, UserStats>();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, UserStats>>(json)
                   ?? new Dictionary<string, UserStats>();
        }

        private void Persist(Dictionary<string, UserStats> data)
        {
            var path = GetStatsFilePath();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private string GetStatsFilePath()
        {
            if (File.Exists(_statsFile))
                return _statsFile;
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _statsFile);
            return path;
        }
    }
}
