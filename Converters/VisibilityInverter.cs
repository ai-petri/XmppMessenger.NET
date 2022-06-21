using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace XmppMessenger.Converters
{
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class VisibilityInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility)) return DependencyProperty.UnsetValue;

            if ((Visibility)value == Visibility.Visible)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
