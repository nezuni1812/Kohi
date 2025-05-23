﻿using Kohi.Models;
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
    public class ExpenseViewModel
    {
        private IDao _dao;
        public FullObservableCollection<ExpenseModel> ExpenseReceipts { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; 
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public ExpenseViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            ExpenseReceipts = new FullObservableCollection<ExpenseModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Expenses.GetCount(); 
            var result = await Task.Run(() => _dao.Expenses.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); 
            ExpenseReceipts.Clear();
            foreach (var item in result)
            {
                item.ExpenseCategory = _dao.ExpenseCategories.GetById(item.ExpenseCategoryId.ToString());
                ExpenseReceipts.Add(item);
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
        public async Task Add(ExpenseModel expensey)
        {
            try
            {
                int result = _dao.Expenses.Insert(expensey);
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
                int result = _dao.Expenses.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, ExpenseModel expensey)
        {
            try
            {
                int result = _dao.Expenses.UpdateById(id, expensey);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<ExpenseModel>> GetAll()
        {
            try
            {
                var Expenses = _dao.Expenses.GetAll(1, 1000); // Đồng bộ
                return Expenses;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
