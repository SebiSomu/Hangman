using Hangman.Models;

namespace Hangman.Services
{
    public interface IStatisticsService
    {
        void UpdateGameStatistics(string username, string category, int level, bool isWon);
        void DeleteUserStatistics(string username);
        void DeleteCategoryStatistics(string categoryName);
        void RenameUsernameStatistics(string oldUsername, string newUsername);
        Dictionary<string, UserStats> GetAllStatistics();
    }
}
