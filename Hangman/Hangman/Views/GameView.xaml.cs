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
            Focus();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
