namespace Hangman.Models
{
    public class UserStats
    {
        public int TotalGamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public List<CategoryStats> CategoryStats { get; set; } = new();
    }

    public class CategoryStats
    {
        public string Category { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int BestLevel { get; set; }
    }
}
