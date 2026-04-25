using System.Windows.Media.Imaging;

namespace Hangman.Services
{
    public interface IAvatarService
    {
        string SaveAvatar(string sourceImagePath, string username);
        string? GetAvatarPath(string? avatarFileName);
        BitmapImage? GetAvatarImage(string? avatarFileName);
        void DeleteAvatar(string? avatarFileName);
        
        string SaveTemporaryAvatar(string sourceImagePath, string username);
        BitmapImage? GetTemporaryAvatarImage(string? tempFileName);
        void DeleteTemporaryAvatar(string? tempFileName);
        string CommitTemporaryAvatar(string? tempFileName, string username);
    }
}
