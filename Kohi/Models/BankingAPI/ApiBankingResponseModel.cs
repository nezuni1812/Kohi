using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kohi.Models.BankingAPI
{
    public class Data : INotifyPropertyChanged
    {
        public int acpId { get; set; }
        public string accountName { get; set; }
        public string qrCode { get; set; }
        public string qrDataURL { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
    public class ApiBankingResponseModel : INotifyPropertyChanged
    {
        public string code { get; set; }
        public string desc { get; set; }
        public Data data { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
