using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CustomerViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CustomerModel> Customers { get; set; }
        public CustomerModel SelectedCustomer { get; set; } 
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public CustomerViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Customers = new FullObservableCollection<CustomerModel>();
            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Customers.GetCount();
            var customersResult = await Task.Run(() => _dao.Customers.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            var allInvoices = await Task.Run(() => _dao.Invoices.GetAll(
                pageNumber: 1,
                pageSize: 1000
            ));

            Customers.Clear();

            foreach (var customer in customersResult)
            {
                var invoicesForCustomer = allInvoices.Where(i => i.CustomerId == customer.Id).ToList();
                customer.Invoices.Clear();
                foreach (var invoice in invoicesForCustomer)
                {
                    customer.Invoices.Add(invoice);
                }
                Debug.WriteLine($"Customer {customer.Id} has {customer.Invoices.Count} invoices");

                Customers.Add(customer);
            }
        }

        public List<CustomerModel> SearchCustomers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<CustomerModel>();

            query = query.ToLower();
            return Customers
                .Where(c => (c.Name != null && c.Name.ToLower().Contains(query)) ||
                            (c.Phone != null && c.Phone.ToLower().Contains(query)))
                .ToList();
        }

        public async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadData(CurrentPage + 1);
            }
        }

        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadData(CurrentPage - 1);
            }
        }

        public async Task GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                await LoadData(page);
            }
        }

        public async Task Add(CustomerModel customer)
        {
            try
            {
                int result = _dao.Customers.Insert(customer);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding customer: {ex.Message}");
            }
        }

        public async Task Delete(string id)
        {
            try
            {
                int result = _dao.Customers.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting customer: {ex.Message}");
            }
        }

        public async Task Update(string id, CustomerModel customer)
        {
            try
            {
                int result = _dao.Customers.UpdateById(id, customer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating customer: {ex.Message}");
            }
        }
        public async Task<List<CustomerModel>> GetAll()
        {
            try
            {
                var Customers = _dao.Customers.GetAll(1, 1000); // Đồng bộ
                return Customers;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}