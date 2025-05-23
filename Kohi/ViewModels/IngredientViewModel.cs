﻿using System;
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

        public IngredientViewModel(bool flag)
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Ingredients = new FullObservableCollection<IngredientModel>();
            PageSize = 1000;
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
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<int> Delete(string id)
        {
            try
            {
                int result = _dao.Ingredients.DeleteById(id);
                await LoadData(CurrentPage);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
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

        //public async Task<List<IngredientModel>> GetAll()
        //{
        //    TotalItems = _dao.Ingredients.GetCount(); // Lấy tổng số khách hàng từ DAO
        //    var result = await Task.Run(() => _dao.Ingredients.GetAll()); // Lấy tất cả dữ liệu mà không phân trang

        //    // Bạn có thể trả về kết quả ngay lập tức hoặc thêm các thao tác khác ở đây
        //    return result.ToList(); // Trả về danh sách các nguyên liệu
        //}
        public async Task<List<IngredientModel>> GetAll()
        {
            try
            {
                var Ingredients = _dao.Ingredients.GetAll(1, 1000); // Đồng bộ
                return Ingredients;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
