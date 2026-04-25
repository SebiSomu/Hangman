using System.IO;
using System.Windows.Media.Imaging;

namespace Hangman.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly string _avatarsFolder;
        private readonly string _tempAvatarsFolder;
        private readonly Dictionary<string, BitmapImage> _imageCache;

        public AvatarService()
        {
            _avatarsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars");
            _tempAvatarsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars", "Temp");
            _imageCache = new Dictionary<string, BitmapImage>();

            if (!Directory.Exists(_avatarsFolder))
                Directory.CreateDirectory(_avatarsFolder);
            if (!Directory.Exists(_tempAvatarsFolder))
                Directory.CreateDirectory(_tempAvatarsFolder);
        }

        public string SaveAvatar(string sourceImagePath, string username)
        {
            var fileName = $"{username}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(sourceImagePath)}";
            var destPath = Path.Combine(_avatarsFolder, fileName);
            File.Copy(sourceImagePath, destPath, true);
            return fileName;
        }

        public string? GetAvatarPath(string? avatarFileName)
        {
            if (string.IsNullOrEmpty(avatarFileName))
                return null;

            var fullPath = Path.Combine(_avatarsFolder, avatarFileName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        public BitmapImage? GetAvatarImage(string? avatarFileName)
        {
            if (string.IsNullOrEmpty(avatarFileName))
                return null;

            if (_imageCache.TryGetValue(avatarFileName, out var cachedImage))
                return cachedImage;

            var fullPath = GetAvatarPath(avatarFileName);
            if (fullPath == null)
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(fullPath);
                image.EndInit();
                image.Freeze();

                _imageCache[avatarFileName] = image;
                return image;
            }
            catch
            {
                return null;
            }
        }

        public void DeleteAvatar(string? avatarFileName)
        {
            if (string.IsNullOrEmpty(avatarFileName))
                return;

            _imageCache.Remove(avatarFileName);

            var fullPath = Path.Combine(_avatarsFolder, avatarFileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public string SaveTemporaryAvatar(string sourceImagePath, string username)
        {
            var fileName = $"temp_{username}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(sourceImagePath)}";
            var destPath = Path.Combine(_tempAvatarsFolder, fileName);
            File.Copy(sourceImagePath, destPath, true);
            return fileName;
        }

        public BitmapImage? GetTemporaryAvatarImage(string? tempFileName)
        {
            if (string.IsNullOrEmpty(tempFileName))
                return null;

            var fullPath = Path.Combine(_tempAvatarsFolder, tempFileName);
            if (!File.Exists(fullPath))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(fullPath);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public void DeleteTemporaryAvatar(string? tempFileName)
        {
            if (string.IsNullOrEmpty(tempFileName))
                return;

            var fullPath = Path.Combine(_tempAvatarsFolder, tempFileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public string CommitTemporaryAvatar(string? tempFileName, string username)
        {
            if (string.IsNullOrEmpty(tempFileName))
                throw new ArgumentException("Temporary file name cannot be null or empty", nameof(tempFileName));

            var tempPath = Path.Combine(_tempAvatarsFolder, tempFileName);
            if (!File.Exists(tempPath))
                throw new FileNotFoundException("Temporary avatar file not found", tempPath);

            var finalFileName = $"{username}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(tempFileName).Replace($"temp_{username}_", "")}";
            var finalPath = Path.Combine(_avatarsFolder, finalFileName);
            
            File.Move(tempPath, finalPath);
            return finalFileName;
        }
    }
}
