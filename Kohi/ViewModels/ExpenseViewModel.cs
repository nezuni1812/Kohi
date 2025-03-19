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
    public class ExpenseViewModel
    {
        private IDao _dao;
        public ObservableCollection<ExpenseModel> ExpenseReceipts { get; set; }

        public ExpenseViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            ExpenseReceipts = new ObservableCollection<ExpenseModel>();

            LoadReceipts();
        }

        private async void LoadReceipts()
        {
            await Task.Delay(0);
            var receipts = _dao.Expenses.GetAll();
            ExpenseReceipts.Clear();

            foreach (var receipt in receipts)
            {
                ExpenseReceipts.Add(receipt);
            }
        }
    }
}
