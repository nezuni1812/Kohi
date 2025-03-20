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
    public class CustomerViewModel
    {
        private IDao _dao;
        public ObservableCollection<CustomerModel> Customers { get; set; }

        public CustomerViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Customers = new ObservableCollection<CustomerModel>();

            LoadData();
        }

        private async void LoadData()
        {
            await Task.Delay(0);
            var customers = _dao.Customers.GetAll();
            Customers.Clear();

            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
    }
}
