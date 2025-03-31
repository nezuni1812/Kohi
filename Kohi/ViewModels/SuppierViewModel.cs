using Kohi.Models;
using Kohi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class SupplierViewModel
    {
        private IDao _dao;
        public ObservableCollection<SupplierModel> Suppliers { get; set; }

        public SupplierViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Suppliers = new ObservableCollection<SupplierModel>();

            LoadData();
        }

        private async void LoadData()
        {
            await Task.Delay(0);
            var suppliers = _dao.Suppliers.GetAll();
            Suppliers.Clear();

            foreach (var supplier in suppliers)
            {
                Suppliers.Add(supplier);
            }
        }
    }
}