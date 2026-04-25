using System.IO;
using System.Text.Json;

namespace Hangman.Services
{
    public class WordRepository : IWordRepository
    {
        private Dictionary<string, List<string>> _categories;
        private readonly Random _random;
        private readonly string _categoriesPath;

        public WordRepository(string categoriesPath = "words.json")
        {
            _categoriesPath = categoriesPath;
            _random = new Random();
            _categories = LoadCategories();
        }

        private Dictionary<string, List<string>> LoadCategories()
        {
            if (!File.Exists(_categoriesPath))
            {
                var defaults = GetDefaultCategories();
                SaveCategories(defaults);
                return defaults;
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

        private static Dictionary<string, List<string>> GetDefaultCategories()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Animals"] = new List<string> { "ELEPHANT", "GIRAFFE", "DOLPHIN", "PENGUIN", "TIGER", "LION", "ZEBRA", "KANGAROO", "OCTOPUS", "BUTTERFLY", "CROCODILE", "RHINOCEROS", "HIPPO", "PANDA", "KOALA", "WOLF", "FOX", "BEAR", "DEER", "EAGLE", "HAWK", "OWL", "PARROT", "PEACOCK", "SWAN", "SHARK", "WHALE", "SEAL", "LIZARD", "SNAKE", "FROG", "TURTLE", "RABBIT", "SQUIRREL", "MOUSE", "BAT", "ANTELOPE", "BUFFALO", "CAMEL", "CHEETAH", "LEOPARD", "JAGUAR", "GORILLA", "MONKEY", "ORANGUTAN", "CHIMPANZEE", "MEERKAT", "RACCOON", "HEDGEHOG" },
                ["Fruits"] = new List<string> { "APPLE", "BANANA", "STRAWBERRY", "WATERMELON", "PINEAPPLE", "MANGO", "BLUEBERRY", "PEACH", "ORANGE", "CHERRY", "GRAPE", "LEMON", "LIME", "PEAR", "PLUM", "KIWI", "PAPAYA", "COCONUT", "AVOCADO", "POMEGRANATE", "APRICOT", "FIG", "DATE", "OLIVE", "GUAVA", "LYCHEE", "PASSIONFRUIT", "DRAGONFRUIT", "CANTALOUPE", "HONEYDEW", "NECTARINE", "TANGERINE", "GRAPEFRUIT", "BLACKBERRY", "RASPBERRY", "CRANBERRY", "CURRANT", "GOOSEBERRY", "MULBERRY", "PERSIMMON", "QUINCE", "JACKFRUIT", "DURIAN", "RAMBUTAN", "LONGAN", "STARFRUIT", "PLANTAIN", "CLEMENTINE", "MANDARIN" },
                ["Countries"] = new List<string> { "AFGHANISTAN", "ALBANIA", "ALGERIA", "ANDORRA", "ANGOLA", "ARGENTINA", "ARMENIA", "AUSTRALIA", "AUSTRIA", "AZERBAIJAN", "BAHAMAS", "BAHRAIN", "BANGLADESH", "BARBADOS", "BELARUS", "BELGIUM", "BELIZE", "BENIN", "BHUTAN", "BOLIVIA", "BOTSWANA", "BRAZIL", "BULGARIA", "BURKINA FASO", "BURUNDI", "CAMBODIA", "CAMEROON", "CANADA", "CHAD", "CHILE", "CHINA", "COLOMBIA", "CONGO", "COSTA RICA", "CROATIA", "CUBA", "CYPRUS", "CZECH REPUBLIC", "DENMARK", "DJIBOUTI", "DOMINICAN REPUBLIC", "ECUADOR", "EGYPT", "EL SALVADOR", "ESTONIA", "ETHIOPIA", "FIJI", "FINLAND", "FRANCE", "GABON", "GAMBIA", "GEORGIA", "GERMANY", "GHANA", "GREECE", "GUATEMALA", "GUINEA", "HAITI", "HONDURAS", "HUNGARY", "ICELAND", "INDIA", "INDONESIA", "IRAN", "IRAQ", "IRELAND", "ISRAEL", "ITALY", "JAMAICA", "JAPAN", "JORDAN", "KAZAKHSTAN", "KENYA", "KUWAIT", "LATVIA", "LEBANON", "LIBERIA", "LIBYA", "LITHUANIA", "LUXEMBOURG", "MADAGASCAR", "MALAWI", "MALAYSIA", "MALDIVES", "MALI", "MALTA", "MAURITANIA", "MAURITIUS", "MEXICO", "MOLDOVA", "MONACO", "MONGOLIA", "MOROCCO", "MOZAMBIQUE", "MYANMAR", "NAMIBIA", "NEPAL", "NETHERLANDS", "NEW ZEALAND", "NICARAGUA", "NIGER", "NIGERIA", "NORTH KOREA", "NORWAY", "OMAN", "PAKISTAN", "PANAMA", "PARAGUAY", "PERU", "PHILIPPINES", "POLAND", "PORTUGAL", "QATAR", "ROMANIA", "RUSSIA", "RWANDA", "SAUDI ARABIA", "SENEGAL", "SERBIA", "SEYCHELLES", "SIERRA LEONE", "SINGAPORE", "SLOVAKIA", "SLOVENIA", "SOMALIA", "SOUTH AFRICA", "SOUTH KOREA", "SPAIN", "SRI LANKA", "SUDAN", "SURINAME", "SWEDEN", "SWITZERLAND", "SYRIA", "TAIWAN", "TAJIKISTAN", "TANZANIA", "THAILAND", "TOGO", "TRINIDAD", "TUNISIA", "TURKEY", "TURKMENISTAN", "UGANDA", "UKRAINE", "UNITED ARAB EMIRATES", "UNITED KINGDOM", "UNITED STATES", "URUGUAY", "UZBEKISTAN", "VENEZUELA", "VIETNAM", "YEMEN", "ZAMBIA", "ZIMBABWE" },
                ["Programming Languages"] = new List<string> { "PYTHON", "JAVASCRIPT", "TYPESCRIPT", "JAVA", "CSHARP", "CPP", "C", "RUBY", "PHP", "SWIFT", "KOTLIN", "GO", "RUST", "SCALA", "R", "MATLAB", "PERL", "HASKELL", "LUA", "DART", "SQL", "BASH", "ASSEMBLY", "COBOL", "FORTRAN", "PASCAL", "ELIXIR", "VISUAL BASIC", "JULIA", "ZIG" },
                ["Sports"] = new List<string> { "BASKETBALL", "FOOTBALL", "TENNIS", "SWIMMING", "VOLLEYBALL", "BASEBALL", "GOLF", "HOCKEY", "CRICKET", "RUGBY", "BOXING", "WRESTLING", "FENCING", "ARCHERY", "SHOOTING", "CYCLING", "RUNNING", "MARATHON", "TRIATHLON", "GYMNASTICS", "WEIGHTLIFTING", "ROWING", "SAILING", "KAYAKING", "SURFING", "DIVING", "SKATEBOARDING", "SNOWBOARDING", "SKIING", "BADMINTON", "SQUASH", "TABLETENNIS", "HANDBALL", "WATERPOLO", "POLO", "LACROSSE", "CHESS", "BOWLING", "DARTS", "BILLIARDS", "SNOOKER", "MOTORSPORT", "FORMULAONE", "NASCAR", "MOTOGP", "BMX", "MOUNTAINBIKING", "HORSERACING", "EQUESTRIAN" }
            };
        }

        public string GetRandomWord(string categoryName)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key != null && _categories.TryGetValue(key, out var words) && words.Count > 0)
                return words[_random.Next(words.Count)];

            var noSpace = categoryName.Replace(" ", "");
            key = _categories.Keys.FirstOrDefault(k => k.Replace(" ", "").Equals(noSpace, StringComparison.OrdinalIgnoreCase));
            if (key != null && _categories.TryGetValue(key, out words) && words.Count > 0)
                return words[_random.Next(words.Count)];

            return "DEFAULT";
        }

        public string GetRandomWordExcluding(string categoryName, string excludeWord)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key == null)
            {
                var noSpace = categoryName.Replace(" ", "");
                key = _categories.Keys.FirstOrDefault(k => k.Replace(" ", "").Equals(noSpace, StringComparison.OrdinalIgnoreCase));
            }

            if (key != null && _categories.TryGetValue(key, out var words) && words.Count > 0)
            {
                var available = words.Where(w => !w.Equals(excludeWord, StringComparison.OrdinalIgnoreCase)).ToList();
                return available.Count > 0 ? available[_random.Next(available.Count)] : words[_random.Next(words.Count)];
            }

            return "DEFAULT";
        }

        public string GetRandomWordFromAllCategories()
        {
            var all = _categories.Values.SelectMany(w => w).ToList();
            return all.Count == 0 ? "DEFAULT" : all[_random.Next(all.Count)];
        }

        public string GetRandomWordFromAllCategoriesExcluding(string excludeWord)
        {
            var all = _categories.Values.SelectMany(w => w)
                .Where(w => !w.Equals(excludeWord, StringComparison.OrdinalIgnoreCase)).ToList();
            return all.Count == 0 ? "DEFAULT" : all[_random.Next(all.Count)];
        }

        public List<string> GetAllCategoryNames() => _categories.Keys.ToList();

        public int GetWordIndex(string categoryName, string word)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key == null)
            {
                var noSpace = categoryName.Replace(" ", "");
                key = _categories.Keys.FirstOrDefault(k => k.Replace(" ", "").Equals(noSpace, StringComparison.OrdinalIgnoreCase));
            }

            if (key != null && _categories.TryGetValue(key, out var words))
            {
                for (int i = 0; i < words.Count; i++)
                {
                    if (words[i].Equals(word, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            return -1;
        }

        public string GetWordByIndex(string categoryName, int index)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key == null)
            {
                var noSpace = categoryName.Replace(" ", "");
                key = _categories.Keys.FirstOrDefault(k => k.Replace(" ", "").Equals(noSpace, StringComparison.OrdinalIgnoreCase));
            }

            if (key != null && _categories.TryGetValue(key, out var words))
            {
                if (index >= 0 && index < words.Count)
                    return words[index];
            }
            return "DEFAULT";
        }

        public string GetCategoryForWord(string word)
        {
            foreach (var category in _categories)
            {
                if (category.Value.Any(w => w.Equals(word, StringComparison.OrdinalIgnoreCase)))
                    return category.Key;
            }
            return string.Empty;
        }

        public List<string> GetWordsForCategory(string categoryName)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            return key != null && _categories.TryGetValue(key, out var words)
                ? words.ToList()
                : new List<string>();
        }

        public bool AddCategory(string name)
        {
            var key = name.Trim();
            if (_categories.Keys.Any(k => k.Equals(key, StringComparison.OrdinalIgnoreCase))) return false;
            _categories[key] = new List<string>();
            SaveCategories(_categories);
            return true;
        }

        public bool DeleteCategory(string name)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (key == null) return false;
            _categories.Remove(key);
            SaveCategories(_categories);
            return true;
        }

        public bool AddWordToCategory(string categoryName, string word)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key == null || !_categories.TryGetValue(key, out var words)) return false;
            var upper = word.Trim().ToUpper();
            if (words.Contains(upper)) return false;
            words.Add(upper);
            SaveCategories(_categories);
            return true;
        }

        public bool DeleteWordFromCategory(string categoryName, string word)
        {
            var key = _categories.Keys.FirstOrDefault(k => k.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (key == null || !_categories.TryGetValue(key, out var words)) return false;
            
            var target = words.FirstOrDefault(w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (target == null) return false;
            
            words.Remove(target);
            SaveCategories(_categories);
            return true;
        }
    }
}
