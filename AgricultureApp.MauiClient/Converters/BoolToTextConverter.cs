using System.Globalization;

namespace AgricultureApp.MauiClient.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parts = parameter.ToString().Split(',');
            return (bool)value ? $"Use {parts[1]}" : $"Use {parts[0]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
