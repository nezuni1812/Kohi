using Kohi.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kohi.Errors
{
    public interface IErrorHandler
    {
        void SetNext(IErrorHandler nextHandler);
        List<string> HandleError(Dictionary<string, string> fields);
    }


}
