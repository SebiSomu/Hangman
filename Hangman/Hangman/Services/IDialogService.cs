using Hangman.Models;

namespace Hangman.Services
{
    /// <summary>
    /// DIP: ViewModel-ul depinde de aceasta interfata, nu de ferestre concrete WPF.
    /// ISP: expune exact operatiunile de dialog necesare - nimic mai mult.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Afiseaza fereastra de introducere parola pentru utilizatorul dat.
        /// Returneaza true daca parola este corecta si utilizatorul s-a autentificat.
        /// </summary>
        bool ShowPasswordDialog(User user);

        /// <summary>
        /// Afiseaza fereastra de selectare joc salvat.
        /// Returneaza jocul ales, sau null daca utilizatorul a anulat.
        /// </summary>
        GameSaveData? ShowSaveGameSelection(
            IEnumerable<GameSaveData> savedGames,
            Action<Guid> onDelete);
    }
}
