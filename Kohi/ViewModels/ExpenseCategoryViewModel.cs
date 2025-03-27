using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
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
            ExpenseCategories.Clear();
            foreach (var expenseCategory in result)
            {
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
    }
}
