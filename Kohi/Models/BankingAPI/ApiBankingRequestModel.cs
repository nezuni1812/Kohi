using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models.BankingAPI
{
    public class ApiBankingRequestModel : INotifyPropertyChanged
    {
        public long accountNo { get; set; }
        public string accountName { get; set; }
        public int acqId { get; set; }
        public int amount { get; set; }
        public string addInfo { get; set; }
        public string format { get; set; }
        public string template { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
