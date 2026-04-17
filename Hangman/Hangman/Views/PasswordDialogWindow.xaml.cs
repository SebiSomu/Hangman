using System.Windows;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class PasswordDialogWindow : Window
    {
        public PasswordDialogWindow(PasswordDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.CloseRequested += result =>
            {
                DialogResult = result;
                Close();
            };

            Loaded += (_, _) => PasswordBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordDialogViewModel vm)
                vm.Password = PasswordBox.Password;
        }
    }
}
