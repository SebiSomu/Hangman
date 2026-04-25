namespace Hangman.Models
{
    public class GameSaveData
    {
        public Guid SaveId { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string SaveName { get; set; } = string.Empty;
        public int WordIndex { get; set; }
        public string Category { get; set; } = string.Empty;
        public string WordCategory { get; set; } = string.Empty;
        public string GuessedLetters { get; set; } = string.Empty;
        public int WrongGuesses { get; set; }
        public int CurrentLevel { get; set; }
        public int TimeRemaining { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
