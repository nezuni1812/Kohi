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
        public FullObservableCollection<InventoryModel> Inventories { get; set; }

        public InventoryCheckViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Inventories = new FullObservableCollection<InventoryModel>();

            LoadData();
        }

        private async void LoadData()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var inventories = _dao.Inventories.GetAll();
            Inventories.Clear();

            foreach (var inventory in inventories)
            {
                Inventories.Add(inventory);
            }
        }
    }
}
