namespace Hangman.Models
{
    public enum WordCategory
    {
        AllCategories,
        Animals,
        Fruits,
        Countries,
        ProgrammingLanguages,
        Sports
    }

    public static class WordCategoryExtensions
    {
        public static string GetDisplayName(this WordCategory category)
        {
            return category switch
            {
                WordCategory.AllCategories => "All Categories",
                WordCategory.Animals => "Animals",
                WordCategory.Fruits => "Fruits",
                WordCategory.Countries => "Countries",
                WordCategory.ProgrammingLanguages => "Programming Languages",
                WordCategory.Sports => "Sports",
                _ => category.ToString()
            };
        }
    }
}
