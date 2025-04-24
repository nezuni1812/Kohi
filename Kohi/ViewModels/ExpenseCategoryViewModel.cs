using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class ExpenseCategoryViewModel
    {
        private IDao _dao;
        public FullObservableCollection<ExpenseCategoryModel> ExpenseCategories { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public ExpenseCategoryViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            ExpenseCategories = new FullObservableCollection<ExpenseCategoryModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.ExpenseCategories.GetCount();
            var result = await Task.Run(() => _dao.ExpenseCategories.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả chi phí một lần
            var allExpenses = await Task.Run(() => _dao.Expenses.GetAll(
                pageNumber: 1,
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát
            ));

            ExpenseCategories.Clear();
            foreach (var expenseCategory in result)
            {
                expenseCategory.Expenses = allExpenses.Where(e => e.ExpenseCategoryId == expenseCategory.Id).ToList();
                Debug.WriteLine($"ExpenseCategory {expenseCategory.Id} has {expenseCategory.Expenses.Count} expenses");
                ExpenseCategories.Add(expenseCategory);
            }
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
        public async Task Add(ExpenseCategoryModel expenseCategory)
        {
            try
            {
                int result = _dao.ExpenseCategories.Insert(expenseCategory);
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
                int result = _dao.ExpenseCategories.DeleteById(id);
                await LoadData(CurrentPage);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task Update(string id, ExpenseCategoryModel expenseCategory)
        {
            try
            {
                int result = _dao.ExpenseCategories.UpdateById(id, expenseCategory);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<ExpenseCategoryModel>> GetAll()
        {
            try
            {
                var ExpenseCategories = _dao.ExpenseCategories.GetAll(1, 1000); // Đồng bộ
                return ExpenseCategories;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
