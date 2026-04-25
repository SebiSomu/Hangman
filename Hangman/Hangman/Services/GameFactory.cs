using Hangman.Models;

namespace Hangman.Services
{
    public class GameFactory : IGameFactory
    {
        private readonly IWordRepository _wordRepository;

        public GameFactory(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public GameState CreateNewGame(string username, string categoryName)
        {
            var word = categoryName.Equals("All Categories", StringComparison.OrdinalIgnoreCase)
                ? _wordRepository.GetRandomWordFromAllCategories()
                : _wordRepository.GetRandomWord(categoryName);

            return new GameState
            {
                Username = username,
                CurrentWord = word,
                Category = categoryName,
                GuessedLetters = string.Empty,
                WrongGuesses = 0,
                CurrentLevel = 1,
                TimeRemaining = 30,
                IsGameOver = false,
                IsWon = false
            };
        }

        public GameState? LoadGameFromSave(GameSaveData saveData)
        {
            if (saveData == null) return null;

            var wordCategory = string.IsNullOrEmpty(saveData.WordCategory) 
                ? saveData.Category 
                : saveData.WordCategory;
            var word = _wordRepository.GetWordByIndex(wordCategory, saveData.WordIndex);

            return new GameState
            {
                Username = saveData.Username,
                CurrentWord = word,
                Category = saveData.Category,
                GuessedLetters = saveData.GuessedLetters,
                WrongGuesses = saveData.WrongGuesses,
                CurrentLevel = saveData.CurrentLevel,
                TimeRemaining = saveData.TimeRemaining,
                IsGameOver = false,
                IsWon = false,
                SaveId = saveData.SaveId
            };
        }
    }
}
