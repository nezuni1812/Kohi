using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    public class IngredientViewModel 
    {
        private IDao _dao;
        public FullObservableCollection<IngredientModel> Ingredients { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public IngredientViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Ingredients = new FullObservableCollection<IngredientModel>();
            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Ingredients.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.Ingredients.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            Ingredients.Clear();
            foreach (var ingredient in result)
            {
                Ingredients.Add(ingredient);
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
        public async Task Add(IngredientModel ingredient)
        {
            try
            {
                int result = _dao.Ingredients.Insert(ingredient);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Delete(string id)
        {
            try
            {
                int result = _dao.Ingredients.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, IngredientModel ingredient)
        {
            try
            {
                int result = _dao.Ingredients.UpdateById(id, ingredient);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
