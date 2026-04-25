using Hangman.Models;

namespace Hangman.Services
{
    public interface ISaveGameService
    {
        Guid SaveGame(GameState gameState, string? saveName = null);
        GameSaveData? LoadSavedGame(Guid saveId);
        List<GameSaveData> GetSavedGamesForUser(string username);
        void DeleteSavedGame(Guid saveId);
        void DeleteAllSavedGamesForUser(string username);
        void DeleteSavedGamesForCategory(string categoryName);
        void RenameUsername(string oldUsername, string newUsername);
    }
}
