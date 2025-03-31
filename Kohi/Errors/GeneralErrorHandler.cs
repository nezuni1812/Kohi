using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Errors
{
    public class GeneralErrorHandler : IErrorHandler
    {
        public void SetNext(IErrorHandler nextHandler) { }

        public List<string> HandleError(Dictionary<string, string> fields)
        {
            return new List<string>(); // Không có lỗi nào
        }
    }

}
