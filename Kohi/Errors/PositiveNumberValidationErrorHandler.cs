using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Errors
{
    public class PositiveNumberValidationErrorHandler : IErrorHandler
    {
        private IErrorHandler _nextHandler;
        private readonly List<string> _fieldsToCheck;

        // Constructor nhận danh sách trường cần kiểm tra
        public PositiveNumberValidationErrorHandler(List<string> fieldsToCheck)
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
                if (fields.ContainsKey(fieldName))
                {
                    // Kiểm tra xem có phải là số hợp lệ không
                    if (double.TryParse(fields[fieldName], out double value))
                    {
                        // Kiểm tra xem có lớn hơn 0 không
                        if (value <= 0)
                        {
                            errors.Add($"⚠ Trường '{fieldName}' phải là số dương (lớn hơn 0).");
                        }
                    }
                    // Không kiểm tra trường hợp không phải số ở đây
                    // vì đã có NumberValidationErrorHandler lo việc đó
                }
            }

            // Chuyển tiếp cho handler tiếp theo trong chuỗi (nếu có)
            if (_nextHandler != null)
                errors.AddRange(_nextHandler.HandleError(fields));

            return errors;
        }
    }
}
