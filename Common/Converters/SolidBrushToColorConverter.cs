using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Converters
{
    [ValueConversion(typeof(SolidColorBrush), typeof(Color))]
    public class SolidBrushToColorConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SolidColorBrush result)) return null;
            return result.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}
