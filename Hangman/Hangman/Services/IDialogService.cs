using Hangman.Models;

namespace Hangman.Services
{
    public interface IDialogService
    {
        bool ShowPasswordDialog(User user);
        GameSaveData? ShowSaveGameSelection(
            IEnumerable<GameSaveData> savedGames,
            Action<Guid> onDelete);
    }
}
