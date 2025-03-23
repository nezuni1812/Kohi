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
    public class InventoryCheckViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CheckInventoryModel> CheckInventories { get; set; }

        public InventoryCheckViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            CheckInventories = new FullObservableCollection<CheckInventoryModel>();

            LoadData();
        }

        private async void LoadData()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var inventories = _dao.CheckInventories.GetAll();
            CheckInventories.Clear();

            foreach (var inventory in inventories)
            {
                CheckInventories.Add(inventory);
            }
        }
    }
}
