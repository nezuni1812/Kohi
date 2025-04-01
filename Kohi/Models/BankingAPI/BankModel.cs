using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models.BankingAPI
{
    public class BankModel : INotifyPropertyChanged
    {
        public string code { get; set; }
        public string desc { get; set; }
        public IList<Datum> data { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }

    public class Datum : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string bin { get; set; }
        public string shortName { get; set; }
        public string logo { get; set; }
        public int transferSupported { get; set; }
        public int lookupSupported { get; set; }
        public string short_name { get; set; }
        public int support { get; set; }
        public int isTransfer { get; set; }
        public string swift_code { get; set; }

        public string custom_name => $"({bin}) {shortName}";
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
