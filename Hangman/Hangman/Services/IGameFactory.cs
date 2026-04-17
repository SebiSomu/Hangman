using Hangman.Models;

namespace Hangman.Services
{
    public interface IGameFactory
    {
        GameState CreateNewGame(string username, string categoryName);
        GameState? LoadGameFromSave(GameSaveData saveData);
    }
}
