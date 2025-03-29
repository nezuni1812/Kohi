using Kohi.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Errors
{
    public class EmptyInputErrorHandler : IErrorHandler
    {
        private IErrorHandler _nextHandler;

        public void SetNext(IErrorHandler nextHandler) => _nextHandler = nextHandler;

        public List<string> HandleError(Dictionary<string, string> fields)
        {
            List<string> errors = new List<string>();

            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    errors.Add($"Trường '{field.Key}' không được để trống.");
                }
            }

            if (_nextHandler != null)
            {
                errors.AddRange(_nextHandler.HandleError(fields));
            }

            return errors;
        }
    }

}
