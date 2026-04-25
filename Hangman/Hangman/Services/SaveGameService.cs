using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class SaveGameService : ISaveGameService
    {
        private readonly string _saveGamesPath;
        private readonly IWordRepository _wordRepository;

        public SaveGameService(string saveGamesPath = "savedgames.json", IWordRepository? wordRepository = null)
        {
            _saveGamesPath = saveGamesPath;
            _wordRepository = wordRepository ?? new WordRepository();
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

            var actualWordCategory = _wordRepository.GetCategoryForWord(gameState.CurrentWord);
            var wordIndex = _wordRepository.GetWordIndex(actualWordCategory, gameState.CurrentWord);

            if (gameState.SaveId.HasValue)
            {
                var existing = userSaves.FirstOrDefault(g => g.SaveId == gameState.SaveId.Value);
                if (existing != null)
                {
                    existing.WordIndex = wordIndex;
                    existing.WordCategory = actualWordCategory;
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
                WordIndex = wordIndex,
                Category = gameState.Category,
                WordCategory = actualWordCategory,
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

        public void DeleteSavedGamesForCategory(string categoryName)
        {
            var allSaves = LoadAllSavedGames();
            bool modified = false;

            foreach (var userSaves in allSaves.Values)
            {
                var toRemove = userSaves
                    .Where(g => g.Category.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var save in toRemove)
                {
                    userSaves.Remove(save);
                    modified = true;
                }
            }

            if (modified)
            {
                Persist(allSaves);
            }
        }

        public void RenameUsername(string oldUsername, string newUsername)
        {
            var allSaves = LoadAllSavedGames();
            if (allSaves.TryGetValue(oldUsername, out var userSaves))
            {
                allSaves.Remove(oldUsername);
                foreach (var save in userSaves)
                {
                    save.Username = newUsername;
                }
                allSaves[newUsername] = userSaves;
                Persist(allSaves);
            }
        }

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
