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

        public InboundViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Inbounds = new FullObservableCollection<InboundModel>();

            LoadData();
        }

        private async void LoadData()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var inbounds = _dao.Inbounds.GetAll();
            Inbounds.Clear();

            foreach (var inbound in inbounds)
            {
                Inbounds.Add(inbound);
            }
        }
    }
}
