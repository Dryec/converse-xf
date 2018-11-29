using System;
using System.Globalization;
using Xamarin.Forms;

namespace Converse.ValueConverters
{
    public class HasUnreadMessagesToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool hasUnreadMessages)
            {
                return hasUnreadMessages ? (Color)App.Current.Resources["UnreadMessagesColor"] : Color.Gray;
            }
            return Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
