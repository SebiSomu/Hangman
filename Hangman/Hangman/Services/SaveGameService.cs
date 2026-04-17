using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class SaveGameService : ISaveGameService
    {
        private readonly string _saveGamesPath;

        public SaveGameService(string saveGamesPath = "savedgames.json")
        {
            _saveGamesPath = saveGamesPath;
        }

        public Guid SaveGame(GameState gameState, string? saveName = null)
        {
            var allSaves = LoadAllSavedGames();
            var username = gameState.Username;

            if (!allSaves.TryGetValue(username, out var userSaves))
            {
                userSaves = new List<GameSaveData>();
                allSaves[username] = userSaves;
            }

            if (gameState.SaveId.HasValue)
            {
                var existing = userSaves.FirstOrDefault(g => g.SaveId == gameState.SaveId.Value);
                if (existing != null)
                {
                    existing.CurrentWord = gameState.CurrentWord;
                    existing.GuessedLetters = gameState.GuessedLetters;
                    existing.WrongGuesses = gameState.WrongGuesses;
                    existing.CurrentLevel = gameState.CurrentLevel;
                    existing.TimeRemaining = gameState.TimeRemaining;
                    existing.SavedAt = DateTime.Now;

                    Persist(allSaves);
                    return gameState.SaveId.Value;
                }
            }

            var saveData = new GameSaveData
            {
                SaveId = Guid.NewGuid(),
                Username = username,
                SaveName = saveName ?? $"Save {userSaves.Count + 1}",
                CurrentWord = gameState.CurrentWord,
                Category = gameState.Category,
                GuessedLetters = gameState.GuessedLetters,
                WrongGuesses = gameState.WrongGuesses,
                CurrentLevel = gameState.CurrentLevel,
                TimeRemaining = gameState.TimeRemaining,
                SavedAt = DateTime.Now
            };

            userSaves.Add(saveData);
            Persist(allSaves);
            return saveData.SaveId;
        }

        public GameSaveData? LoadSavedGame(Guid saveId)
        {
            var allSaves = LoadAllSavedGames();
            return allSaves.Values.SelectMany(list => list).FirstOrDefault(g => g.SaveId == saveId);
        }

        public List<GameSaveData> GetSavedGamesForUser(string username)
        {
            var allSaves = LoadAllSavedGames();
            if (allSaves.TryGetValue(username, out var userSaves))
            {
                return userSaves.OrderByDescending(g => g.SavedAt).ToList();
            }
            return new List<GameSaveData>();
        }

        public void DeleteSavedGame(Guid saveId)
        {
            var allSaves = LoadAllSavedGames();
            bool removed = false;

            foreach (var userList in allSaves.Values)
            {
                var item = userList.FirstOrDefault(g => g.SaveId == saveId);
                if (item != null)
                {
                    userList.Remove(item);
                    removed = true;
                    break;
                }
            }

            if (removed)
            {
                Persist(allSaves);
            }
        }

        public void DeleteAllSavedGamesForUser(string username)
        {
            var allSaves = LoadAllSavedGames();
            if (allSaves.Remove(username))
            {
                Persist(allSaves);
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private Dictionary<string, List<GameSaveData>> LoadAllSavedGames()
        {
            if (!File.Exists(_saveGamesPath)) 
                return new Dictionary<string, List<GameSaveData>>();

            var json = File.ReadAllText(_saveGamesPath);
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, List<GameSaveData>>();

            try
            {
                if (json.TrimStart().StartsWith("{"))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, List<GameSaveData>>>(json) 
                           ?? new Dictionary<string, List<GameSaveData>>();
                }

                if (json.TrimStart().StartsWith("["))
                {
                    var oldList = JsonSerializer.Deserialize<List<GameSaveData>>(json);
                    if (oldList != null)
                    {
                        return oldList.GroupBy(g => g.Username)
                                      .ToDictionary(g => g.Key, g => g.ToList());
                    }
                }
            }
            catch { }

            return new Dictionary<string, List<GameSaveData>>();
        }

        private void Persist(Dictionary<string, List<GameSaveData>> data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveGamesPath, json);
        }
    }
}
