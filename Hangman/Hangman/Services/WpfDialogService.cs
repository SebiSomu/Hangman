using System.Windows;
using Hangman.Models;
using Hangman.Views;
using Hangman.ViewModels;

namespace Hangman.Services
{
    public class WpfDialogService : IDialogService
    {
        private readonly IUserService _userService;

        public WpfDialogService(IUserService userService)
        {
            _userService = userService;
        }

        public bool ShowPasswordDialog(User user)
        {
            var vm = new PasswordDialogViewModel(user, _userService);
            var window = new PasswordDialogWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };
            return window.ShowDialog() == true;
        }

        public GameSaveData? ShowSaveGameSelection(
            IEnumerable<GameSaveData> savedGames,
            Action<Guid> onDelete)
        {
            var vm = new SaveGameSelectionViewModel(savedGames, onDelete);
            var window = new SaveGameSelectionWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };

            return window.ShowDialog() == true ? vm.SelectedSave : null;
        }
    }
}
