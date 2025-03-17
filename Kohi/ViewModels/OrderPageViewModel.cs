using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class OrderPageViewModel
    {
        public CategoryViewModel CategoryViewModel { get; set; }
        public ProductViewModel ProductViewModel { get; set; }
        public OrderPageViewModel()
        {
            CategoryViewModel = new CategoryViewModel();
            ProductViewModel = new ProductViewModel();
        }
    }
}
