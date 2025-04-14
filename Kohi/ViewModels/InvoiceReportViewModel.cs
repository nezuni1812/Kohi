using Kohi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InvoiceReportViewModel : INotifyPropertyChanged
    {
        private InvoiceViewModel _invoiceViewModel;
        private DateTimeOffset _startDate;
        private DateTimeOffset _endDate;
        private string _selectedTimeRange = "Hôm nay";
        private List<InvoiceModel>? _invoices;

        public ObservableCollection<PaymentMethodChartData> PaymentMethodData { get; set; }
        public ObservableCollection<OrderTypeChartData> OrderTypeData { get; set; }
        public ObservableCollection<InvoiceModel> FilteredInvoices { get; set; }

        public string SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                _selectedTimeRange = value;
                OnPropertyChanged(nameof(SelectedTimeRange));
            }
        }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public InvoiceReportViewModel()
        {
            _invoiceViewModel = new InvoiceViewModel();
            PaymentMethodData = new ObservableCollection<PaymentMethodChartData>();
            OrderTypeData = new ObservableCollection<OrderTypeChartData>();
            FilteredInvoices = new ObservableCollection<InvoiceModel>();
            StartDate = DateTimeOffset.Now;
            EndDate = DateTimeOffset.Now;
            InitializeData();
        }

        private async void InitializeData()
        {
            _invoices = await _invoiceViewModel.GetAll();
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices found during initialization.");
                _invoices = new List<InvoiceModel>();
            }
            Debug.WriteLine($"Initialized with {(_invoices?.Count ?? 0)} invoices");
            UpdateChartData(SelectedTimeRange);
        }

        public void UpdateChartData(string timeRange)
        {
            SelectedTimeRange = timeRange;
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices available to process.");
                _invoices = new List<InvoiceModel>();
            }
            Debug.WriteLine($"Processing {(_invoices?.Count ?? 0)} invoices for time range: {timeRange}");

            DateTimeOffset startDate, endDate;
            bool groupByMonth = false;

            switch (timeRange)
            {
                case "Hôm nay":
                    startDate = DateTimeOffset.Now.Date;
                    endDate = startDate.AddDays(1);
                    break;
                case "Tuần này":
                    startDate = DateTimeOffset.Now.Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek);
                    endDate = startDate.AddDays(7);
                    break;
                case "Tháng này":
                    startDate = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    endDate = startDate.AddMonths(1);
                    break;
                case "Năm này":
                    startDate = new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    endDate = startDate.AddYears(1);
                    groupByMonth = true;
                    break;
                case "Tùy chỉnh":
                    startDate = StartDate.Date;
                    endDate = EndDate.Date.Add(new TimeSpan(23, 59, 59));
                    groupByMonth = (endDate - startDate).TotalDays > 30;
                    //System.Diagnostics.Debug.WriteLine($"startDate: {startDate}, endDate: {endDate}, groupByMonth: {groupByMonth}");
                    break;
                default:
                    return;
            }

            var filteredInvoices = _invoices
                .Where(i => i.CreatedAt.HasValue && i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .ToList();
            Debug.WriteLine($"Filtered {filteredInvoices.Count} invoices between {startDate} and {endDate}");

            if (FilteredInvoices == null)
            {
                FilteredInvoices = new ObservableCollection<InvoiceModel>();
            }
            FilteredInvoices.Clear();
            foreach (var invoice in filteredInvoices)
            {
                FilteredInvoices.Add(invoice);
            }
            OnPropertyChanged(nameof(FilteredInvoices));

            if (PaymentMethodData == null)
            {
                PaymentMethodData = new ObservableCollection<PaymentMethodChartData>();
            }
            PaymentMethodData.Clear();
            var paymentMethodGroups = filteredInvoices
                .GroupBy(i => i.PaymentMethod)
                .Select(g => new PaymentMethodChartData(g.Key, g.Count()))
                .ToList();
            foreach (var data in paymentMethodGroups)
            {
                PaymentMethodData.Add(data);
            }
            OnPropertyChanged(nameof(PaymentMethodData));

            if (OrderTypeData == null)
            {
                OrderTypeData = new ObservableCollection<OrderTypeChartData>();
            }
            OrderTypeData.Clear();
            var orderTypeGroups = filteredInvoices
                .GroupBy(i => i.OrderType)
                .Select(g => new OrderTypeChartData(g.Key, g.Count()))
                .ToList();
            foreach (var data in orderTypeGroups)
            {
                OrderTypeData.Add(data);
            }
            OnPropertyChanged(nameof(OrderTypeData));
        }

        public async void RefreshData()
        {
            _invoices = await _invoiceViewModel.GetAll();
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices found during refresh.");
                _invoices = new List<InvoiceModel>();
            }
            Debug.WriteLine($"Refreshed with {_invoices.Count} invoices");
            UpdateChartData(SelectedTimeRange);
        }

        public async Task<InvoiceModel> GetInvoiceDetailsAsync(int invoiceId)
        {
            try
            {
                var detailedInvoice = await _invoiceViewModel.GetDetailsById(invoiceId);
                if (detailedInvoice == null)
                {
                    Debug.WriteLine($"No details found for Invoice ID: {invoiceId}");
                    return null;
                }

                Debug.WriteLine($"Successfully fetched Invoice ID: {detailedInvoice.Id} with {detailedInvoice.InvoiceDetails.Count} details");
                return detailedInvoice;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching details for Invoice ID: {invoiceId}. Error: {ex.Message}");
                return null;
            }
        }
    }

    public class PaymentMethodChartData
    {
        public string PaymentMethod { get; set; }
        public int Count { get; set; }

        public PaymentMethodChartData(string paymentMethod, int count)
        {
            PaymentMethod = paymentMethod;
            Count = count;
        }
    }

    public class OrderTypeChartData
    {
        public string OrderType { get; set; }
        public int Count { get; set; }

        public OrderTypeChartData(string orderType, int count)
        {
            OrderType = orderType;
            Count = count;
        }
    }
}