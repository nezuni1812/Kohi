using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public int PageSize { get; set; } = 3; // Mặc định 3 khách hàng mỗi trang
        public int TotalItems { get; set; } // Tổng số khách hàng
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
            TotalItems = _dao.Customers.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.Customers.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            Customers.Clear();
            foreach (var customer in result)
            {
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
    }
}
