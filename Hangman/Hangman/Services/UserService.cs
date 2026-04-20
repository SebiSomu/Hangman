using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Hangman.Models;

namespace Hangman.Services
{
    public class UserService : IUserService
    {
        private readonly string _usersFilePath;
        private readonly IStatisticsService _statisticsService;
        private List<User> _users;

        public UserService(string usersFilePath = "users.json", IStatisticsService? statisticsService = null)
        {
            _usersFilePath = usersFilePath;
            _statisticsService = statisticsService;
            _users = LoadUsers();
        }

        private List<User> LoadUsers()
        {
            if (!File.Exists(_usersFilePath))
                return new List<User>();

            var json = File.ReadAllText(_usersFilePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFilePath, json);
        }

        public bool UserExists(string username)
        {
            return _users.Any(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
        }

        public bool ValidateUser(string username, string password)
        {
            return _users.Any(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase) 
                                   && u.Password == password);
        }

        public User? GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(User user)
        {
            _users.Add(user);
            SaveUsers();
        }

        public void UpdateUser(User user)
        {
            var existing = _users.FirstOrDefault(u =>
                u.Username.Equals(user.Username, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.AvatarFileName = user.AvatarFileName;
                existing.Password = user.Password;
                SaveUsers();
            }
        }

        public bool UpdateUsername(string oldUsername, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                return false;

            if (UserExists(newUsername))
                return false;

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(oldUsername, System.StringComparison.OrdinalIgnoreCase));
            
            if (user == null)
                return false;

            user.Username = newUsername;
            SaveUsers();
            
            _statisticsService?.RenameUsernameStatistics(oldUsername, newUsername);
            
            return true;
        }

        public void DeleteUser(string username)
        {
            _users.RemoveAll(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            SaveUsers();
        }

        public List<User> GetAllUsers()
        {
            return new List<User>(_users);
        }
    }
}
