using System;
using Windows.UI.Xaml.Data;

namespace Kohi.Views.Converter
{
    public class DateTimeConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
            }
            return "Không có ngày";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (DateTime.TryParse(value?.ToString(), out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}