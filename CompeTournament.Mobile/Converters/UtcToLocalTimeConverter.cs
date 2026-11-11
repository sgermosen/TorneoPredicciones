using System.Globalization;

namespace CompeTournament.Mobile.Converters
{
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
        {
            DateTime { Kind: DateTimeKind.Local } local => local,
            DateTime date => DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime(),
            _ => value
        };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
