using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class GameService
    {
        private Dictionary<string, List<string>> _categories;
        private readonly Random _random;
        private readonly string _saveGamesPath;
        private readonly string _categoriesPath;

        public GameService(string saveGamesPath = "savedgames.json", string categoriesPath = "categories.json")
        {
            _saveGamesPath = saveGamesPath;
            _categoriesPath = categoriesPath;
            _random = new Random();
            _categories = LoadCategories();
        }

        private Dictionary<string, List<string>> LoadCategories()
        {
            if (!File.Exists(_categoriesPath))
            {
                var defaultCategories = GetDefaultCategories();
                SaveCategories(defaultCategories);
                return defaultCategories;
            }

            var json = File.ReadAllText(_categoriesPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            return loaded ?? GetDefaultCategories();
        }

        private void SaveCategories(Dictionary<string, List<string>> categories)
        {
            var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_categoriesPath, json);
        }

        private Dictionary<string, List<string>> GetDefaultCategories()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Animals"] = new List<string> { "ELEPHANT", "GIRAFFE", "DOLPHIN", "PENGUIN", "TIGER", "LION", "ZEBRA", "KANGAROO", "OCTOPUS", "BUTTERFLY", "CROCODILE", "RHINOCEROS", "HIPPO", "PANDA", "KOALA", "WOLF", "FOX", "BEAR", "DEER", "EAGLE", "HAWK", "OWL", "PARROT", "PEACOCK", "SWAN", "SHARK", "WHALE", "SEAL", "LIZARD", "SNAKE", "FROG", "TURTLE", "RABBIT", "SQUIRREL", "MOUSE", "BAT", "ANTELOPE", "BUFFALO", "CAMEL", "CHEETAH", "LEOPARD", "JAGUAR", "GORILLA", "MONKEY", "ORANGUTAN", "CHIMPANZEE", "MEERKAT", "RACCOON", "HEDGEHOG" },
                ["Fruits"] = new List<string> { "APPLE", "BANANA", "STRAWBERRY", "WATERMELON", "PINEAPPLE", "MANGO", "BLUEBERRY", "PEACH", "ORANGE", "CHERRY", "GRAPE", "LEMON", "LIME", "PEAR", "PLUM", "KIWI", "PAPAYA", "COCONUT", "AVOCADO", "POMEGRANATE", "APRICOT", "FIG", "DATE", "OLIVE", "GUAVA", "LYCHEE", "PASSIONFRUIT", "DRAGONFRUIT", "CANTALOUPE", "HONEYDEW", "NECTARINE", "TANGERINE", "GRAPEFRUIT", "BLACKBERRY", "RASPBERRY", "CRANBERRY", "CURRANT", "GOOSEBERRY", "MULBERRY", "PERSIMMON", "QUINCE", "JACKFRUIT", "DURIAN", "RAMBUTAN", "LONGAN", "STARFRUIT", "PLANTAIN", "CLEMENTINE", "MANDARIN" },
                ["Countries"] = new List<string> { "AFGHANISTAN", "ALBANIA", "ALGERIA", "ANDORRA", "ANGOLA", "ARGENTINA", "ARMENIA", "AUSTRALIA", "AUSTRIA", "AZERBAIJAN", "BAHAMAS", "BAHRAIN", "BANGLADESH", "BARBADOS", "BELARUS", "BELGIUM", "BELIZE", "BENIN", "BHUTAN", "BOLIVIA", "BOTSWANA", "BRAZIL", "BULGARIA", "BURKINA FASO", "BURUNDI", "CAMBODIA", "CAMEROON", "CANADA", "CHAD", "CHILE", "CHINA", "COLOMBIA", "CONGO", "COSTA RICA", "CROATIA", "CUBA", "CYPRUS", "CZECH REPUBLIC", "DENMARK", "DJIBOUTI", "DOMINICAN REPUBLIC", "ECUADOR", "EGYPT", "EL SALVADOR", "ESTONIA", "ETHIOPIA", "FIJI", "FINLAND", "FRANCE", "GABON", "GAMBIA", "GEORGIA", "GERMANY", "GHANA", "GREECE", "GUATEMALA", "GUINEA", "HAITI", "HONDURAS", "HUNGARY", "ICELAND", "INDIA", "INDONESIA", "IRAN", "IRAQ", "IRELAND", "ISRAEL", "ITALY", "JAMAICA", "JAPAN", "JORDAN", "KAZAKHSTAN", "KENYA", "KUWAIT", "LATVIA", "LEBANON", "LIBERIA", "LIBYA", "LITHUANIA", "LUXEMBOURG", "MADAGASCAR", "MALAWI", "MALAYSIA", "MALDIVES", "MALI", "MALTA", "MAURITANIA", "MAURITIUS", "MEXICO", "MOLDOVA", "MONACO", "MONGOLIA", "MOROCCO", "MOZAMBIQUE", "MYANMAR", "NAMIBIA", "NEPAL", "NETHERLANDS", "NEW ZEALAND", "NICARAGUA", "NIGER", "NIGERIA", "NORTH KOREA", "NORWAY", "OMAN", "PAKISTAN", "PANAMA", "PARAGUAY", "PERU", "PHILIPPINES", "POLAND", "PORTUGAL", "QATAR", "ROMANIA", "RUSSIA", "RWANDA", "SAUDI ARABIA", "SENEGAL", "SERBIA", "SEYCHELLES", "SIERRA LEONE", "SINGAPORE", "SLOVAKIA", "SLOVENIA", "SOMALIA", "SOUTH AFRICA", "SOUTH KOREA", "SPAIN", "SRI LANKA", "SUDAN", "SURINAME", "SWEDEN", "SWITZERLAND", "SYRIA", "TAIWAN", "TAJIKISTAN", "TANZANIA", "THAILAND", "TOGO", "TRINIDAD", "TUNISIA", "TURKEY", "TURKMENISTAN", "UGANDA", "UKRAINE", "UNITED ARAB EMIRATES", "UNITED KINGDOM", "UNITED STATES", "URUGUAY", "UZBEKISTAN", "VENEZUELA", "VIETNAM", "YEMEN", "ZAMBIA", "ZIMBABWE" },
                ["ProgrammingLanguages"] = new List<string> { "PYTHON", "JAVASCRIPT", "TYPESCRIPT", "JAVA", "CSHARP", "CPP", "C", "RUBY", "PHP", "SWIFT", "KOTLIN", "GO", "RUST", "SCALA", "R", "MATLAB", "PERL", "HASKELL", "LUA", "DART", "SQL", "BASH", "ASSEMBLY", "COBOL", "FORTRAN", "PASCAL", "ELIXIR", "VISUAL BASIC", "JULIA", "ZIG" },
                ["Sports"] = new List<string> { "BASKETBALL", "FOOTBALL", "TENNIS", "SWIMMING", "VOLLEYBALL", "BASEBALL", "GOLF", "HOCKEY", "CRICKET", "RUGBY", "BOXING", "WRESTLING", "FENCING", "ARCHERY", "SHOOTING", "CYCLING", "RUNNING", "MARATHON", "TRIATHLON", "GYMNASTICS", "WEIGHTLIFTING", "ROWING", "SAILING", "KAYAKING", "SURFING", "DIVING", "SKATEBOARDING", "SNOWBOARDING", "SKIING", "BADMINTON", "SQUASH", "TABLETENNIS", "HANDBALL", "WATERPOLO", "POLO", "LACROSSE", "CHESS", "BOWLING", "DARTS", "BILLIARDS", "SNOOKER", "MOTORSPORT", "FORMULAONE", "NASCAR", "MOTOGP", "BMX", "MOUNTAINBIKING", "HORSERACING", "EQUESTRIAN" }
            };
        }

        public string GetRandomWord(string categoryName)
        {
            // Try exact match first
            if (_categories.TryGetValue(categoryName, out var words) && words.Count > 0)
            {
                return words[_random.Next(words.Count)];
            }
            
            var noSpaceName = categoryName.Replace(" ", "");
            if (_categories.TryGetValue(noSpaceName, out words) && words.Count > 0)
            {
                return words[_random.Next(words.Count)];
            }
            
            return "DEFAULT";
        }

        public string GetRandomWordExcluding(string categoryName, string excludeWord)
        {
            if (_categories.TryGetValue(categoryName, out var words) && words.Count > 0)
            {
                var availableWords = words.Where(w => !w.Equals(excludeWord, StringComparison.OrdinalIgnoreCase)).ToList();
                if (availableWords.Count > 0)
                {
                    return availableWords[_random.Next(availableWords.Count)];
                }
                return words[_random.Next(words.Count)];
            }
            
            var noSpaceName = categoryName.Replace(" ", "");
            if (_categories.TryGetValue(noSpaceName, out words) && words.Count > 0)
            {
                var availableWords = words.Where(w => !w.Equals(excludeWord, StringComparison.OrdinalIgnoreCase)).ToList();
                if (availableWords.Count > 0)
                {
                    return availableWords[_random.Next(availableWords.Count)];
                }
                return words[_random.Next(words.Count)];
            }
            
            return "DEFAULT";
        }

        public List<string> GetAllCategoryNames()
        {
            return _categories.Keys.ToList();
        }

        public List<string> GetWordsForCategory(string categoryName)
        {
            if (_categories.TryGetValue(categoryName, out var words))
            {
                return words.ToList();
            }
            return new List<string>();
        }

        public bool AddCategory(string name)
        {
            var key = name.Trim();
            if (_categories.ContainsKey(key))
            {
                return false;
            }
            _categories[key] = new List<string>();
            SaveCategories(_categories);
            return true;
        }

        public bool DeleteCategory(string name)
        {
            if (!_categories.ContainsKey(name))
            {
                return false;
            }
            _categories.Remove(name);
            SaveCategories(_categories);
            return true;
        }

        public bool AddWordToCategory(string categoryName, string word)
        {
            if (!_categories.TryGetValue(categoryName, out var words))
            {
                return false;
            }
            var upperWord = word.Trim().ToUpper();
            if (words.Contains(upperWord))
            {
                return false;
            }
            words.Add(upperWord);
            SaveCategories(_categories);
            return true;
        }

        public bool DeleteWordFromCategory(string categoryName, string word)
        {
            if (!_categories.TryGetValue(categoryName, out var words))
            {
                return false;
            }
            words.Remove(word);
            SaveCategories(_categories);
            return true;
        }

        public string GetRandomWord(WordCategory category)
        {
            return GetRandomWord(category.GetDisplayName());
        }

        public string GetRandomWordExcluding(WordCategory category, string excludeWord)
        {
            return GetRandomWordExcluding(category.GetDisplayName(), excludeWord);
        }

        public List<WordCategory> GetAllCategories()
        {
            var result = new List<WordCategory>();
            foreach (var cat in _categories.Keys)
            {
                var normalized = cat.Replace(" ", "");
                if (Enum.TryParse<WordCategory>(normalized, out var enumCat) && enumCat != WordCategory.AllCategories)
                {
                    result.Add(enumCat);
                }
            }
            return result.Count > 0 ? result : new List<WordCategory> { WordCategory.Animals };
        }

        public string GetRandomWordFromAllCategories()
        {
            var allWords = _categories.Values.SelectMany(w => w).ToList();
            if (allWords.Count == 0) return "DEFAULT";
            return allWords[_random.Next(allWords.Count)];
        }

        public string GetRandomWordFromAllCategoriesExcluding(string excludeWord)
        {
            var allWords = _categories.Values.SelectMany(w => w)
                .Where(w => !w.Equals(excludeWord, StringComparison.OrdinalIgnoreCase)).ToList();
            if (allWords.Count == 0) return "DEFAULT";
            return allWords[_random.Next(allWords.Count)];
        }

        public Guid SaveGame(GameState gameState, string? saveName = null)
        {
            var savedGames = LoadAllSavedGames();

            if (gameState.SaveId.HasValue)
            {
                var existingSave = savedGames.FirstOrDefault(g => g.SaveId == gameState.SaveId.Value);
                if (existingSave != null)
                {
                    existingSave.CurrentWord = gameState.CurrentWord;
                    existingSave.GuessedLetters = gameState.GuessedLetters;
                    existingSave.WrongGuesses = gameState.WrongGuesses;
                    existingSave.CurrentLevel = gameState.CurrentLevel;
                    existingSave.TimeRemaining = gameState.TimeRemaining;
                    existingSave.SavedAt = DateTime.Now;

                    var json = JsonSerializer.Serialize(savedGames, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_saveGamesPath, json);
                    return gameState.SaveId.Value;
                }
            }

            var saveData = new GameSaveData
            {
                SaveId = Guid.NewGuid(),
                Username = gameState.Username,
                SaveName = saveName ?? $"Save {savedGames.Count(g => g.Username == gameState.Username) + 1}",
                CurrentWord = gameState.CurrentWord,
                Category = gameState.Category,
                GuessedLetters = gameState.GuessedLetters,
                WrongGuesses = gameState.WrongGuesses,
                CurrentLevel = gameState.CurrentLevel,
                TimeRemaining = gameState.TimeRemaining,
                SavedAt = DateTime.Now
            };

            savedGames.Add(saveData);

            var jsonNew = JsonSerializer.Serialize(savedGames, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveGamesPath, jsonNew);
            return saveData.SaveId;
        }

        public GameSaveData? LoadSavedGame(Guid saveId)
        {
            var savedGames = LoadAllSavedGames();
            return savedGames.FirstOrDefault(g => g.SaveId == saveId);
        }

        public List<GameSaveData> GetSavedGamesForUser(string username)
        {
            var savedGames = LoadAllSavedGames();
            return savedGames.Where(g => g.Username == username).OrderByDescending(g => g.SavedAt).ToList();
        }

        public void DeleteSavedGame(Guid saveId)
        {
            var savedGames = LoadAllSavedGames();
            savedGames.RemoveAll(g => g.SaveId == saveId);
            var json = JsonSerializer.Serialize(savedGames, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveGamesPath, json);
        }

        public void DeleteAllSavedGamesForUser(string username)
        {
            var savedGames = LoadAllSavedGames();
            savedGames.RemoveAll(g => g.Username == username);
            var json = JsonSerializer.Serialize(savedGames, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveGamesPath, json);
        }

        private List<GameSaveData> LoadAllSavedGames()
        {
            if (!File.Exists(_saveGamesPath))
                return new List<GameSaveData>();

            var json = File.ReadAllText(_saveGamesPath);
            return JsonSerializer.Deserialize<List<GameSaveData>>(json) ?? new List<GameSaveData>();
        }

        public GameState CreateNewGame(string username, WordCategory category)
        {
            var displayName = category.GetDisplayName();
            var word = category == WordCategory.AllCategories 
                ? GetRandomWordFromAllCategories() 
                : GetRandomWord(displayName);
            return new GameState
            {
                Username = username,
                CurrentWord = word,
                Category = displayName,
                GuessedLetters = string.Empty,
                WrongGuesses = 0,
                CurrentLevel = 1,
                TimeRemaining = 30,
                IsGameOver = false,
                IsWon = false
            };
        }

        public GameState? LoadGameFromSave(GameSaveData saveData)
        {
            if (saveData == null) return null;

            return new GameState
            {
                Username = saveData.Username,
                CurrentWord = saveData.CurrentWord,
                Category = saveData.Category,
                GuessedLetters = saveData.GuessedLetters,
                WrongGuesses = saveData.WrongGuesses,
                CurrentLevel = saveData.CurrentLevel,
                TimeRemaining = saveData.TimeRemaining,
                IsGameOver = false,
                IsWon = false,
                SaveId = saveData.SaveId
            };
        }

        public void UpdateGameStatistics(string username, string category, int level, bool isWon)
        {
            var statsFile = "gamestats.json";
            Dictionary<string, UserStats> allStats;
            
            if (File.Exists(statsFile))
            {
                var statsJson = File.ReadAllText(statsFile);
                allStats = JsonSerializer.Deserialize<Dictionary<string, UserStats>>(statsJson) ?? new Dictionary<string, UserStats>();
            }
            else
            {
                allStats = new Dictionary<string, UserStats>();
            }

            if (!allStats.TryGetValue(username, out var userStats))
            {
                userStats = new UserStats();
                allStats[username] = userStats;
            }

            userStats.TotalGamesPlayed++;
            if (isWon)
            {
                userStats.GamesWon++;
            }
            else
            {
                userStats.GamesLost++;
            }

            var categoryStat = userStats.CategoryStats.FirstOrDefault(cs => cs.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            if (categoryStat == null)
            {
                categoryStat = new CategoryStats { Category = category };
                userStats.CategoryStats.Add(categoryStat);
            }

            categoryStat.GamesPlayed++;
            if (isWon)
            {
                categoryStat.GamesWon++;
            }
            if (level > categoryStat.BestLevel)
            {
                categoryStat.BestLevel = level;
            }

            var outputJson = JsonSerializer.Serialize(allStats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(statsFile, outputJson);
        }

        public void DeleteUserStatistics(string username)
        {
            var statsFile = "gamestats.json";
            if (!File.Exists(statsFile)) return;

            var statsJson = File.ReadAllText(statsFile);
            var allStats = JsonSerializer.Deserialize<Dictionary<string, UserStats>>(statsJson) ?? new Dictionary<string, UserStats>();
            
            if (allStats.Remove(username))
            {
                var json = JsonSerializer.Serialize(allStats, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(statsFile, json);
            }
        }

        public class UserStats
        {
            public int TotalGamesPlayed { get; set; }
            public int GamesWon { get; set; }
            public int GamesLost { get; set; }
            public List<CategoryStats> CategoryStats { get; set; } = new();
        }

        public class CategoryStats
        {
            public string Category { get; set; } = string.Empty;
            public int GamesPlayed { get; set; }
            public int GamesWon { get; set; }
            public int BestLevel { get; set; }
        }
    }
}
