using System.Windows;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class SaveGameSelectionWindow : Window
    {
        public SaveGameSelectionWindow(SaveGameSelectionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.CloseRequested += result =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
