using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Errors
{
    public class DateRangeValidationErrorHandler : IErrorHandler
    {
        private IErrorHandler _nextHandler;
        private readonly string _startDateFieldName;
        private readonly string _endDateFieldName;

        // Constructor nhận tên của 2 trường ngày cần kiểm tra
        public DateRangeValidationErrorHandler(string startDateFieldName, string endDateFieldName)
        {
            _startDateFieldName = startDateFieldName;
            _endDateFieldName = endDateFieldName;
        }

        public void SetNext(IErrorHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public List<string> HandleError(Dictionary<string, string> fields)
        {
            List<string> errors = new List<string>();

            if (fields.ContainsKey(_startDateFieldName) && fields.ContainsKey(_endDateFieldName))
            {
                bool isStartValid = DateTime.TryParse(fields[_startDateFieldName], out DateTime startDate);
                bool isEndValid = DateTime.TryParse(fields[_endDateFieldName], out DateTime endDate);

                if (!isStartValid)
                {
                    errors.Add($"⚠ Trường '{_startDateFieldName}' phải là ngày giờ hợp lệ.");
                }
                if (!isEndValid)
                {
                    errors.Add($"⚠ Trường '{_endDateFieldName}' phải là ngày giờ hợp lệ.");
                }

                if (isStartValid && isEndValid && endDate < startDate)
                {
                    errors.Add($"⚠ Thời gian kết thúc '{_endDateFieldName}' phải lớn hơn thời gian bắt đầu '{_startDateFieldName}'.");
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
