namespace Hangman.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; }

        public User() { }

        public User(string username, string password, string? avatarFileName = null)
        {
            Username = username;
            Password = password;
            AvatarFileName = avatarFileName;
        }
    }
}
