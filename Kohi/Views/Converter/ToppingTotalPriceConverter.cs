using Kohi.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Views.Converter
{
    public class ToppingTotalPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderToppingModel topping && topping.ProductVariant != null)
            {
                float totalPrice = topping.Quantity * topping.ProductVariant.Price;
                var moneyConverter = new MoneyFormatConverter();
                return moneyConverter.Convert(totalPrice, typeof(string), null, language);
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
