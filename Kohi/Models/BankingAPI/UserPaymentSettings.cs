using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models.BankingAPI
{
    public class UserPaymentSettings
    {
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string BankBin { get; set; } // Dùng bin để tìm lại bank
        public string Template { get; set; }
        public string Address { get; set; } 
        public string Street { get; set; }    
        public string Ward { get; set; }      
        public string District { get; set; }  
        public string City { get; set; }      

    }

}
