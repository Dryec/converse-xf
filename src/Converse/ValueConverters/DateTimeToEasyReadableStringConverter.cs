using System;
using System.Globalization;
using Xamarin.Forms;

namespace Converse.ValueConverters
{
    public class DateTimeToEasyReadableStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DateTime time)
            {
                var now = DateTime.Now;
                if(time.Year == now.Year && time.Month == now.Month && time.Day == now.Day)
                {
                    return time.ToShortTimeString();
                }
                else if(time.Year == now.Year && time.Month == now.Month && time.Day == now.Day-1)
                {
                    return "Yesterday";
                }
                else if(time.Year == now.Year && time.Month == now.Month && time.Day < now.Day)
                {
                    return time.ToString("dddd");
                }
                else
                {
                    return time.ToShortDateString();
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
