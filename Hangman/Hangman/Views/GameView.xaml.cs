using System.Windows.Controls;
using System.Windows.Input;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (DataContext is GameViewModel vm)
                {
                    vm.FocusRequested += (sender, args) => Focus();
                }
            };
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Focus the control so keyboard input works
            Focus();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard shortcut: pressing A-Z keys triggers letter guess
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char letter = (char)('A' + (e.Key - Key.A));
                if (DataContext is GameViewModel vm)
                {
                    vm.ProcessKeyboardLetter(letter);
                    e.Handled = true;
                }
            }
        }
    }
}
