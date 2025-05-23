﻿using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Views.Converter
{
    public class MoneyFormatConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        private readonly CultureInfo culture = CultureInfo.GetCultureInfo("vi-VN");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "0 đ";
            }

            try
            {
                float floatValue = System.Convert.ToSingle(value);
                return floatValue.ToString("#,##0 đ", culture.NumberFormat);
            }
            catch
            {
                return "0 đ";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Xử lý khi người dùng nhập dữ liệu (nếu cần)
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                // Loại bỏ ký tự "đ" và các ký tự không phải số để parse lại
                str = str.Replace("đ", "").Replace(",", "").Trim();
                if (float.TryParse(str, NumberStyles.Any, culture, out float result))
                {
                    return result;
                }
            }
            return 0f;
        }
    }
}
