using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kohi.View.Converter
{
    class AbsolutePathConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            string filename = (string)value;
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string path = $"{folder}Assets/{filename}";
            return path;
        }

        public object ConvertBack(object value, Type type, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    
    
}
