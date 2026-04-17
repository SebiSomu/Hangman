using Hangman.Models;

namespace Hangman.Services
{
    public interface IUserService
    {
        bool UserExists(string username);
        bool ValidateUser(string username, string password);
        User? GetUser(string username);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(string username);
        List<User> GetAllUsers();
    }
}
