using Hangman.Models;

namespace Hangman.Services
{
    public interface IStatisticsService
    {
        void UpdateGameStatistics(string username, string category, int level, bool isWon);
        void DeleteUserStatistics(string username);
        Dictionary<string, UserStats> GetAllStatistics();
    }
}
