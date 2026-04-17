using Hangman.Models;

namespace Hangman.Services
{
    public interface IWordRepository
    {
        string GetRandomWord(string categoryName);
        string GetRandomWordExcluding(string categoryName, string excludeWord);
        string GetRandomWordFromAllCategories();
        string GetRandomWordFromAllCategoriesExcluding(string excludeWord);

        List<string> GetAllCategoryNames();
        List<string> GetWordsForCategory(string categoryName);
        bool AddCategory(string name);
        bool DeleteCategory(string name);
        bool AddWordToCategory(string categoryName, string word);
        bool DeleteWordFromCategory(string categoryName, string word);
    }
}
