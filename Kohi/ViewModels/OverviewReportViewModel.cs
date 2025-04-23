using Kohi.Models;
using Kohi.Views.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Globalization;

namespace Kohi.ViewModels
{
    public class OverviewReportViewModel : INotifyPropertyChanged
    {
        private InvoiceViewModel _invoiceViewModel;
        private ExpenseViewModel _expenseViewModel;
        private ExpenseCategoryViewModel _expenseCategoryViewModel;
        private DateTimeOffset _startDate;
        private DateTimeOffset _endDate;
        private string _selectedTimeRange = "Hôm nay";
        private float _totalRevenue;
        private float _maxProfit;
        private float _totalExpense;
        private float _profitInterval;
        private List<InvoiceModel>? _invoices;
        private List<ExpenseModel>? _expenses;

        public ObservableCollection<ChartData> ChartData { get; set; }
        public ObservableCollection<ExpenseCategoryChartData> ExpenseCategoryData { get; set; } // Dữ liệu cho biểu đồ tròn
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
        public float TotalProfit { get; set; }
        public float MaxProfit
        {
            get => _maxProfit;
            set
            {
                _maxProfit = value;
                OnPropertyChanged(nameof(MaxProfit));
            }
        }
        public float ProfitInterval
        {
            get => _profitInterval;
            set
            {
                _profitInterval = value;
                OnPropertyChanged(nameof(ProfitInterval));
            }
        }
        public float TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged(nameof(TotalRevenue));
            }
        }

        public float TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                OnPropertyChanged(nameof(TotalExpense));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public OverviewReportViewModel()
        {
            _invoiceViewModel = new InvoiceViewModel();
            _expenseViewModel = new ExpenseViewModel();
            _expenseCategoryViewModel = new ExpenseCategoryViewModel();
            ChartData = new ObservableCollection<ChartData>();
            ExpenseCategoryData = new ObservableCollection<ExpenseCategoryChartData>();
            StartDate = DateTimeOffset.Now;
            EndDate = DateTimeOffset.Now;
            InitializeData();
        }

        private async void InitializeData()
        {
            // Tải invoices
            _invoices = await _invoiceViewModel.GetAllWithDetails();
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices found during initialization.");
                _invoices = new List<InvoiceModel>();
            }
            Debug.WriteLine($"Initialized with {(_invoices?.Count ?? 0)} invoices");

            // Tải expenses
            _expenses = await _expenseViewModel.GetAll();
            if (_expenses == null || !_expenses.Any())
            {
                Debug.WriteLine("No expenses found during initialization.");
                _expenses = new List<ExpenseModel>();
            }
            Debug.WriteLine($"Initialized with {(_expenses?.Count ?? 0)} expenses");

            UpdateChartData(SelectedTimeRange);
        }

        public void UpdateChartData(string timeRange)
        {
            SelectedTimeRange = timeRange;
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices available to process.");
                return;
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
                    startDate = StartDate;
                    endDate = EndDate.Date.Add(new TimeSpan(23, 59, 59));
                    groupByMonth = (endDate - startDate).TotalDays > 30;
                    break;
                default:
                    return;
            }

            // Lọc invoices theo khoảng thời gian
            var filteredInvoices = _invoices
                .Where(i => i.CreatedAt.HasValue && i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .ToList();
            Debug.WriteLine($"Filtered {filteredInvoices.Count} invoices between {startDate} and {endDate}");

            // Lọc expenses theo khoảng thời gian
            var filteredExpenses = _expenses
                .Where(e => e.ExpenseDate.HasValue && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .ToList();
            Debug.WriteLine($"Filtered {filteredExpenses.Count} expenses between {startDate} and {endDate}");

            // Tính doanh thu và lợi nhuận
            float totalProfit = 0;
            float totalRevenue = 0;
            foreach (var invoice in filteredInvoices)
            {
                if (invoice.InvoiceDetails == null || !invoice.InvoiceDetails.Any())
                {
                    Debug.WriteLine($"Invoice {invoice.Id} has no details.");
                    continue;
                }

                totalRevenue += invoice.TotalAmount;
                foreach (var detail in invoice.InvoiceDetails)
                {
                    var variant = detail.ProductVariant;
                    if (variant == null)
                    {
                        Debug.WriteLine($"InvoiceDetail {detail.Id} has no ProductVariant.");
                        continue;
                    }

                    float profit = (variant.Price - variant.Cost) * detail.Quantity;
                    totalProfit += profit;
                }
            }

            float totalExpense = filteredExpenses.Sum(e => e.Amount);

            TotalRevenue = filteredInvoices.Any() ? totalRevenue : 0f;
            TotalProfit = filteredInvoices.Any() ? totalProfit : 0f;
            TotalExpense = filteredExpenses.Any() ? totalExpense : 0f;
            OnPropertyChanged(nameof(TotalProfit));
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalExpense));

            // Tính chi phí theo danh mục
            var expenseCategories = _expenseCategoryViewModel.ExpenseCategories.ToList();
            ExpenseCategoryData.Clear();
            foreach (var category in expenseCategories)
            {
                var expensesForCategory = filteredExpenses
                    .Where(e => e.ExpenseCategoryId == category.Id)
                    .ToList();
                float totalAmount = expensesForCategory.Sum(e => e.Amount);
                if (totalAmount > 0) // Chỉ thêm danh mục có chi phí
                {
                    ExpenseCategoryData.Add(new ExpenseCategoryChartData(category.CategoryName, totalAmount));
                }
            }
            OnPropertyChanged(nameof(ExpenseCategoryData));

            // Cập nhật dữ liệu cho biểu đồ cột
            ChartData.Clear();
            if (groupByMonth)
            {
                var monthlyData = filteredInvoices
                    .GroupBy(i => new { i.CreatedAt.Value.Year, i.CreatedAt.Value.Month })
                    .Select(g => new
                    {
                        Month = new DateTimeOffset(g.Key.Year, g.Key.Month, 1, 0, 0, 0, TimeSpan.Zero),
                        Profit = g.Sum(i => i.InvoiceDetails.Sum(d => (d.ProductVariant.Price - d.ProductVariant.Cost) * d.Quantity)),
                        Revenue = g.Sum(i => i.TotalAmount)
                    })
                    .OrderBy(g => g.Month);

                foreach (var data in monthlyData)
                {
                    ChartData.Add(new ChartData($"{data.Month.Month}/{data.Month.Year}", data.Profit, data.Revenue));
                }
            }
            else
            {
                if (timeRange == "Hôm nay")
                {
                    var hourlyData = filteredInvoices
                        .GroupBy(i => i.CreatedAt.Value.Hour)
                        .Select(g => new
                        {
                            Hour = g.Key,
                            Profit = g.Sum(i => i.InvoiceDetails.Sum(d => (d.ProductVariant.Price - d.ProductVariant.Cost) * d.Quantity)),
                            Revenue = g.Sum(i => i.TotalAmount)
                        })
                        .OrderBy(g => g.Hour);

                    foreach (var data in hourlyData)
                    {
                        ChartData.Add(new ChartData($"{data.Hour}h", data.Profit, data.Revenue));
                    }
                }
                else
                {
                    var dailyData = filteredInvoices
                        .GroupBy(i => i.CreatedAt.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Profit = g.Sum(i => i.InvoiceDetails.Sum(d => (d.ProductVariant.Price - d.ProductVariant.Cost) * d.Quantity)),
                            Revenue = g.Sum(i => i.TotalAmount)
                        })
                        .OrderBy(g => g.Date);

                    foreach (var data in dailyData)
                    {
                        ChartData.Add(new ChartData($"{data.Date.Day}/{data.Date.Month}", data.Profit, data.Revenue));
                    }
                }
            }

            if (ChartData.Any())
            {
                MaxProfit = (float)Math.Ceiling(Math.Max(ChartData.Max(d => d.Profit), ChartData.Max(d => d.Revenue)) / 10000) * 10000;
                ProfitInterval = MaxProfit / 5;
            }
            else
            {
                MaxProfit = 10000;
                ProfitInterval = 2000;
            }

            OnPropertyChanged(nameof(ChartData));
            OnPropertyChanged(nameof(MaxProfit));
            OnPropertyChanged(nameof(ProfitInterval));
        }

        public async void RefreshData()
        {
            _invoices = await _invoiceViewModel.GetAllWithDetails();
            if (_invoices == null || !_invoices.Any())
            {
                Debug.WriteLine("No invoices found during refresh.");
                _invoices = new List<InvoiceModel>();
            }
            Debug.WriteLine($"Refreshed with {_invoices.Count} invoices");

            _expenses = await _expenseViewModel.GetAll();
            if (_expenses == null || !_expenses.Any())
            {
                Debug.WriteLine("No expenses found during refresh.");
                _expenses = new List<ExpenseModel>();
            }
            Debug.WriteLine($"Refreshed with {_expenses.Count} expenses");

            await _expenseCategoryViewModel.LoadData();
            UpdateChartData(SelectedTimeRange);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartData
    {
        public string Label { get; set; }
        public float Profit { get; set; }
        public float Revenue { get; set; }

        public ChartData(string label, float profit, float revenue)
        {
            Label = label;
            Profit = profit;
            Revenue = revenue;
        }
    }

    public class ExpenseCategoryChartData
    {
        public string CategoryName { get; set; }
        public float Amount { get; set; }

        public ExpenseCategoryChartData(string categoryName, float amount)
        {
            CategoryName = categoryName;
            Amount = amount;
        }
    }
}