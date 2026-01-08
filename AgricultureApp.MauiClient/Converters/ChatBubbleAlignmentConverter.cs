using System.Globalization;

namespace AgricultureApp.MauiClient.Converters
{
    public class ChatBubbleAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var author = value?.ToString()?.ToLowerInvariant();
            return author == "you" ? LayoutOptions.End : LayoutOptions.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}