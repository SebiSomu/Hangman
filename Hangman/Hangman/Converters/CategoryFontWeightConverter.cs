using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hangman.Converters
{
    public class CategoryFontWeightConverter : IValueConverter
    {
        public static readonly CategoryFontWeightConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selected && parameter is string param)
                return selected == param ? FontWeights.Bold : FontWeights.Normal;

            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
