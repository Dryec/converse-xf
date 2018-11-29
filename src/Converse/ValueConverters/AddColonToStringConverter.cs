using System;
using System.Globalization;
using Xamarin.Forms;

namespace Converse.ValueConverters
{
    public class AddColonToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string text)
            {
                return text + ":";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
