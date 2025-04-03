using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InboundViewModel
    {
        private IDao _dao;
        public FullObservableCollection<InboundModel> Inbounds { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; 
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InboundViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Inbounds = new FullObservableCollection<InboundModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Inbounds.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.Inbounds.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            Inbounds.Clear();
            foreach (var item in result)
            {
                Inbounds.Add(item);
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
        public async Task Add(InboundModel inbound)
        {
            try
            {
                int result = _dao.Inbounds.Insert(inbound);
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
                int result = _dao.Inbounds.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, InboundModel inbound)
        {
            try
            {
                int result = _dao.Inbounds.UpdateById(id, inbound);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
