using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hangman.Converters
{
    public class StringToNullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
                return Visibility.Collapsed;  // Hide when has value
            return Visibility.Visible;  // Show when null/empty
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
