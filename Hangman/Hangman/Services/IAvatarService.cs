using System.Windows.Media.Imaging;

namespace Hangman.Services
{
    public interface IAvatarService
    {
        string SaveAvatar(string sourceImagePath, string username);
        string? GetAvatarPath(string? avatarFileName);
        BitmapImage? GetAvatarImage(string? avatarFileName);
        void DeleteAvatar(string? avatarFileName);
    }
}
