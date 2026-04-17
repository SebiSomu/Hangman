namespace Hangman.Models
{
    /// <summary>
    /// Aggregated play statistics for a single user.
    /// Moved out of GameService (SRP) — Models should own their own data shapes.
    /// </summary>
    public class UserStats
    {
        public int TotalGamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public List<CategoryStats> CategoryStats { get; set; } = new();
    }

    /// <summary>
    /// Per-category statistics for a user.
    /// </summary>
    public class CategoryStats
    {
        public string Category { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int BestLevel { get; set; }
    }
}
