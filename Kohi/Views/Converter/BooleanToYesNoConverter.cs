using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Views.Converter
{
    public class BooleanToYesNoConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                // Chuyển true thành "Có", false thành "Không"
                return boolValue ? "Có" : "Không";
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Xử lý khi người dùng nhập dữ liệu (nếu cần)
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                // Chuyển "Có" thành true, "Không" thành false
                if (str.Trim().Equals("Có", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (str.Trim().Equals("Không", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return false; // Giá trị mặc định nếu không parse được
        }
    }
}
