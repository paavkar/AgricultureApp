using AgricultureApp.MauiClient.Resources.Strings;
using System.Globalization;

namespace AgricultureApp.MauiClient.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                // Resource key format: {EnumType}_{EnumValue}
                // Example: FieldStatus_Active, SoilType_Loamy
                var key = $"{enumValue.GetType().Name}_{enumValue}";

                var localized = AppResources.ResourceManager.GetString(key, culture);

                return localized ?? enumValue.ToString();
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
