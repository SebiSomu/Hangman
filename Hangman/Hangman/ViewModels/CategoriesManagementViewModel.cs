using System.Collections.ObjectModel;
using System.Windows.Input;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class CategoriesManagementViewModel : ViewModelBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly IStatisticsService _statisticsService;
        private readonly ISaveGameService _saveGameService;
        private ObservableCollection<CategoryViewModel> _categories;
        private CategoryViewModel? _selectedCategory;
        private string _newCategoryName = string.Empty;
        private string _newWord = string.Empty;

        public ObservableCollection<CategoryViewModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CategoryViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    OnPropertyChanged(nameof(CanAddWord));
                    OnPropertyChanged(nameof(CanDeleteCategory));
                    OnPropertyChanged(nameof(CanDeleteWord));
                }
            }
        }

        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                if (SetProperty(ref _newCategoryName, value))
                    OnPropertyChanged(nameof(CanAddCategory));
            }
        }

        public string NewWord
        {
            get => _newWord;
            set
            {
                if (SetProperty(ref _newWord, value))
                    OnPropertyChanged(nameof(CanAddWord));
            }
        }

        public bool CanAddCategory => !string.IsNullOrWhiteSpace(NewCategoryName);
        public bool CanDeleteCategory => SelectedCategory != null;
        public bool CanAddWord => SelectedCategory != null && !string.IsNullOrWhiteSpace(NewWord);
        public bool CanDeleteWord => SelectedCategory != null && SelectedCategory.SelectedWord != null;

        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand AddWordCommand { get; }
        public ICommand DeleteWordCommand { get; }
        public ICommand BackCommand { get; }

        public event EventHandler? BackRequested;

        public CategoriesManagementViewModel(IWordRepository wordRepository, IStatisticsService statisticsService, ISaveGameService saveGameService)
        {
            _wordRepository = wordRepository;
            _statisticsService = statisticsService;
            _saveGameService = saveGameService;
            _categories = new ObservableCollection<CategoryViewModel>();

            AddCategoryCommand = new RelayCommand(_ => AddCategory(), _ => CanAddCategory);
            DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory(), _ => CanDeleteCategory);
            AddWordCommand = new RelayCommand(_ => AddWord(), _ => CanAddWord);
            DeleteWordCommand = new RelayCommand(_ => DeleteWord(), _ => CanDeleteWord);
            BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            foreach (var name in _wordRepository.GetAllCategoryNames())
            {
                var words = _wordRepository.GetWordsForCategory(name);
                Categories.Add(new CategoryViewModel(name, words));
            }
        }

        private void AddCategory()
        {
            var name = NewCategoryName.Trim();
            if (_wordRepository.AddCategory(name))
            {
                Categories.Add(new CategoryViewModel(name, new List<string>()));
                NewCategoryName = string.Empty;
            }
            else
            {
                System.Windows.MessageBox.Show("Category already exists!", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void DeleteCategory()
        {
            if (SelectedCategory == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete '{SelectedCategory.Name}' and all its words?\n\nThis will also delete all statistics and saved games for this category for all users.",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _statisticsService.DeleteCategoryStatistics(SelectedCategory.Name);
                _saveGameService.DeleteSavedGamesForCategory(SelectedCategory.Name);
                _wordRepository.DeleteCategory(SelectedCategory.Name);
                Categories.Remove(SelectedCategory);
                SelectedCategory = null;
            }
        }

        private void AddWord()
        {
            if (SelectedCategory == null) return;

            var word = NewWord.Trim().ToUpper();
            if (_wordRepository.AddWordToCategory(SelectedCategory.Name, word))
            {
                SelectedCategory.Words.Add(word);
                NewWord = string.Empty;
            }
            else
            {
                System.Windows.MessageBox.Show("Word already exists in this category!", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void DeleteWord()
        {
            if (SelectedCategory?.SelectedWord == null) return;
            _wordRepository.DeleteWordFromCategory(SelectedCategory.Name, SelectedCategory.SelectedWord);
            SelectedCategory.Words.Remove(SelectedCategory.SelectedWord);
            SelectedCategory.SelectedWord = null;
        }
    }

    public class CategoryViewModel : ViewModelBase
    {
        private string _name;
        private ObservableCollection<string> _words;
        private string? _selectedWord;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<string> Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }

        public string? SelectedWord
        {
            get => _selectedWord;
            set => SetProperty(ref _selectedWord, value);
        }

        public CategoryViewModel(string name, List<string> words)
        {
            _name = name;
            _words = new ObservableCollection<string>(words);
        }
    }
}
