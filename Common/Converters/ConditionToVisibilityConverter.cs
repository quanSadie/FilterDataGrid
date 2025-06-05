using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.Converters
{
    public class ConditionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FilterCondition condition)
            {
                return condition != FilterCondition.None && condition != FilterCondition.ShowOnlySelected
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
