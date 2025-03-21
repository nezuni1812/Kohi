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
    public class OutboundViewModel
    {
        private IDao _dao;
        public FullObservableCollection<OutboundModel> Outbounds { get; set; }

        public OutboundViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Outbounds = new FullObservableCollection<OutboundModel>();

            LoadData();
        }

        private async void LoadData()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var outbounds = _dao.Outbounds.GetAll();
            Outbounds.Clear();

            foreach (var outbound in outbounds)
            {
                Outbounds.Add(outbound);
            }
        }
    }
}
