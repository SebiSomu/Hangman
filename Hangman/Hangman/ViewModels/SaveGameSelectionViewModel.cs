using System.Collections.ObjectModel;
using Hangman.Models;

namespace Hangman.ViewModels
{
    public class SaveGameSelectionViewModel : ViewModelBase
    {
        private readonly Action<Guid> _onDelete;
        private GameSaveData? _selectedSave;

        public event Action<bool>? CloseRequested;

        public ObservableCollection<GameSaveData> SavedGames { get; }

        public GameSaveData? SelectedSave
        {
            get => _selectedSave;
            set => SetProperty(ref _selectedSave, value);
        }

        public bool HasSaves => SavedGames.Count > 0;

        public RelayCommand LoadCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public SaveGameSelectionViewModel(
            IEnumerable<GameSaveData> savedGames,
            Action<Guid> onDelete)
        {
            _onDelete = onDelete;
            SavedGames = new ObservableCollection<GameSaveData>(savedGames);

            _selectedSave = SavedGames.FirstOrDefault();

            LoadCommand = new RelayCommand(
                _ => CloseRequested?.Invoke(true),
                _ => SelectedSave != null);

            DeleteCommand = new RelayCommand(
                _ => DeleteSelected(),
                _ => SelectedSave != null);

            CancelCommand = new RelayCommand(
                _ => CloseRequested?.Invoke(false));
        }

        private void DeleteSelected()
        {
            if (SelectedSave == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Delete save '{SelectedSave.SaveName}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            _onDelete(SelectedSave.SaveId);
            SavedGames.Remove(SelectedSave);
            SelectedSave = SavedGames.FirstOrDefault();
            OnPropertyChanged(nameof(HasSaves));
  
            if (SavedGames.Count == 0)
                CloseRequested?.Invoke(false);
        }
    }
}
