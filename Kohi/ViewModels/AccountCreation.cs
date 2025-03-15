using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kohi.Models;

namespace Kohi.ViewModels
{
    internal class AccountCreation
    {
        public CustomerModel newAccount { get; set; }
        public AccountCreation() {
            newAccount = new CustomerModel();
        }
    }
}
