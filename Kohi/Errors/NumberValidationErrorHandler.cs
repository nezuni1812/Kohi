using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Errors
{
    public class NumberValidationErrorHandler : IErrorHandler
    {
        private IErrorHandler _nextHandler;
        private readonly List<string> _fieldsToCheck;

        // Constructor nhận danh sách trường cần kiểm tra
        public NumberValidationErrorHandler(List<string> fieldsToCheck)
        {
            _fieldsToCheck = fieldsToCheck ?? new List<string>(); // Tránh null
        }

        public void SetNext(IErrorHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public List<string> HandleError(Dictionary<string, string> fields)
        {
            List<string> errors = new List<string>();

            foreach (var fieldName in _fieldsToCheck)
            {
                if (fields.ContainsKey(fieldName) && !double.TryParse(fields[fieldName], out _))
                {
                    errors.Add($"⚠ Trường '{fieldName}' phải là số hợp lệ.");
                }
            }

            if (_nextHandler != null)
                errors.AddRange(_nextHandler.HandleError(fields));

            return errors;
        }
    }
}
