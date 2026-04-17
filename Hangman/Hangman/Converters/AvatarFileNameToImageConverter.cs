using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Hangman.Services;

namespace Hangman.Converters
{
    public class AvatarFileNameToImageConverter : IValueConverter
    {
        private static readonly AvatarService _avatarService = new AvatarService();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName && !string.IsNullOrEmpty(fileName))
            {
                return _avatarService.GetAvatarImage(fileName);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
