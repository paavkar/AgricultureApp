using System.Globalization;

namespace AgricultureApp.MauiClient.Converters
{
    public class ChatBubbleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var author = value?.ToString()?.ToLowerInvariant();

            return author switch
            {
                "you" => Color.FromArgb("#DCF8C6"),
                "assistant" => Color.FromArgb("#E5E5EA"),
                "system" => Colors.OrangeRed,
                _ => Colors.LightGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}