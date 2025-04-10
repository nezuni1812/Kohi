﻿using Kohi.Models;
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
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; 
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
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

            // Lấy tất cả hóa đơn từ API
            var allInvoices = await Task.Run(() => _dao.Invoices.GetAll(
                pageNumber: 1,
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát
            ));

            Customers.Clear();

            foreach (var customer in customersResult)
            {
                // Lọc hóa đơn theo CustomerId và gán vào khách hàng
                var invoicesForCustomer = allInvoices.Where(i => i.CustomerId == customer.Id).ToList();
                customer.Invoices.Clear(); // Xóa danh sách cũ (nếu có)
                foreach (var invoice in invoicesForCustomer)
                {
                    customer.Invoices.Add(invoice);
                }
                Debug.WriteLine($"Customer {customer.Id} has {customer.Invoices.Count} invoices");

                Customers.Add(customer);
            }
        }

        // Phương thức để chuyển đến trang tiếp theo
        public async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadData(CurrentPage + 1);
            }
        }

        // Phương thức để quay lại trang trước
        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadData(CurrentPage - 1);
            }
        }

        // Phương thức để chuyển đến trang cụ thể
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

            }
        }
    }
}
