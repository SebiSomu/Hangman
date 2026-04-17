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
            var savedGames = LoadAllSavedGames();

            if (gameState.SaveId.HasValue)
            {
                var existing = savedGames.FirstOrDefault(g => g.SaveId == gameState.SaveId.Value);
                if (existing != null)
                {
                    existing.CurrentWord = gameState.CurrentWord;
                    existing.GuessedLetters = gameState.GuessedLetters;
                    existing.WrongGuesses = gameState.WrongGuesses;
                    existing.CurrentLevel = gameState.CurrentLevel;
                    existing.TimeRemaining = gameState.TimeRemaining;
                    existing.SavedAt = DateTime.Now;

                    Persist(savedGames);
                    return gameState.SaveId.Value;
                }
            }

            var saveData = new GameSaveData
            {
                SaveId = Guid.NewGuid(),
                Username = gameState.Username,
                SaveName = saveName ?? $"Save {savedGames.Count(g => g.Username == gameState.Username) + 1}",
                CurrentWord = gameState.CurrentWord,
                Category = gameState.Category,
                GuessedLetters = gameState.GuessedLetters,
                WrongGuesses = gameState.WrongGuesses,
                CurrentLevel = gameState.CurrentLevel,
                TimeRemaining = gameState.TimeRemaining,
                SavedAt = DateTime.Now
            };

            savedGames.Add(saveData);
            Persist(savedGames);
            return saveData.SaveId;
        }

        public GameSaveData? LoadSavedGame(Guid saveId)
        {
            var savedGames = LoadAllSavedGames();
            return savedGames.FirstOrDefault(g => g.SaveId == saveId);
        }

        public List<GameSaveData> GetSavedGamesForUser(string username)
        {
            return LoadAllSavedGames()
                .Where(g => g.Username == username)
                .OrderByDescending(g => g.SavedAt)
                .ToList();
        }

        public void DeleteSavedGame(Guid saveId)
        {
            var savedGames = LoadAllSavedGames();
            savedGames.RemoveAll(g => g.SaveId == saveId);
            Persist(savedGames);
        }

        public void DeleteAllSavedGamesForUser(string username)
        {
            var savedGames = LoadAllSavedGames();
            savedGames.RemoveAll(g => g.Username == username);
            Persist(savedGames);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private List<GameSaveData> LoadAllSavedGames()
        {
            if (!File.Exists(_saveGamesPath)) return new List<GameSaveData>();
            var json = File.ReadAllText(_saveGamesPath);
            return JsonSerializer.Deserialize<List<GameSaveData>>(json) ?? new List<GameSaveData>();
        }

        private void Persist(List<GameSaveData> data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveGamesPath, json);
        }
    }
}
