using Kohi.Models;
using Kohi.Views.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Globalization;
using AutoGen.Gemini;
using AutoGen.Core;
using Syncfusion.UI.Xaml.Chat;
using System.Text;
using System.Threading.Tasks;

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
        private ObservableCollection<object> chats;
        private bool isTrue = false;
        private TypingIndicator typingIndicator;
        private IEnumerable<string> suggestion;
        private Author currentUser;
        private static string apiKey = AppConfig.Configuration["GeminiAPIKey"];

        private MiddlewareStreamingAgent<GeminiChatAgent> geminiAgent = new GeminiChatAgent(
            name: "gemini",
            model: "gemini-2.0-flash",
            apiKey: apiKey,
            systemMessage: "You are a helpful assistant for a POS coffee shop application for Vietnamese so most currency data will be in VNĐ." +
" There will be a text version of what the user is seeing on the screen included in the prompt, please make use of it as much as you can. " +
"Please noted if the user ask for something other than coffeeshop related like creating a programe with any sort of language, decline. If you are sure that the question is out of the scope of the provided data, you can just say you dont know")
        .RegisterMessageConnector()
        .RegisterPrintMessage();

        public ObservableCollection<ChartData> ChartData { get; set; }
        public ObservableCollection<ExpenseCategoryChartData> ExpenseCategoryData { get; set; }
        public ObservableCollection<object> Chats
        {
            get => chats;
            set
            {
                chats = value;
                OnPropertyChanged(nameof(Chats));
            }
        }
        public bool IsTrue
        {
            get => isTrue;
            set
            {
                isTrue = value;
                OnPropertyChanged(nameof(IsTrue));
            }
        }
        public TypingIndicator TypingIndicator
        {
            get => typingIndicator;
            set
            {
                typingIndicator = value;
                OnPropertyChanged(nameof(TypingIndicator));
            }
        }
        public IEnumerable<string> Suggestion
        {
            get => suggestion;
            set
            {
                suggestion = value;
                OnPropertyChanged(nameof(Suggestion));
            }
        }
        public Author CurrentUser
        {
            get => currentUser;
            set
            {
                currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
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
            Chats = new ObservableCollection<object>();
            Suggestion = new ObservableCollection<string>
            {
                "Danh mục chi phí lớn nhất là gì?",
                "Xu hướng lợi nhuận trong khoảng thời gian này?",
                "Có vấn đề gì cần chú ý trong doanh thu và chi phí không?",
                "So sánh doanh thu và lợi nhuận theo ngày"
            };
            CurrentUser = new Author { Name = "User" };
            TypingIndicator = new TypingIndicator { Author = new Author { Name = "Bot" } };
            Chats.CollectionChanged += Chats_CollectionChanged;

            if (apiKey is null)
            {
                Debug.WriteLine("Please set GOOGLE_GEMINI_API_KEY environment variable.");
            }

            InitializeData();
        }

        private async void Chats_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems?[0] as ITextMessage;
            if (item != null && item.Author.Name == CurrentUser.Name)
            {
                Debug.WriteLine("User text: " + item.Text);
                IsTrue = true;

                try
                {
                    string chartDataText = await GetChartDataAsText();
                    string messageWithData = $"{item.Text}\n\nHere is the chart data the user is seeing, please answer according to the data in it:\n{chartDataText}";
                    var reply = await geminiAgent.SendAsync(messageWithData);
                    Debug.WriteLine("Response: " + reply.GetContent());

                    Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage
                    {
                        Author = new Author { Name = "Bot" },
                        DateTime = DateTime.Now,
                        Text = reply.GetContent()
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing AI response: {ex.Message}");
                    Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage
                    {
                        Author = new Author { Name = "Bot" },
                        DateTime = DateTime.Now,
                        Text = "Xin lỗi, đã xảy ra lỗi khi xử lý yêu cầu của bạn."
                    });
                }
                finally
                {
                    IsTrue = false;
                }
            }
        }

        public void HandleSuggestionClicked(string suggestionText)
        {
            Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage
            {
                Author = CurrentUser,
                DateTime = DateTime.Now,
                Text = suggestionText
            });
        }

        private async Task<string> GetChartDataAsText()
        {
            var text = new StringBuilder();

            if (StartDate != null && EndDate != null)
            {
                text.AppendLine($"Data Range: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
            }
            else
            {
                text.AppendLine("Data Range: All time");
            }

            text.AppendLine($"\nSummary:");
            text.AppendLine($"  Total Revenue: {TotalRevenue:C}");
            text.AppendLine($"  Total Profit: {TotalProfit:C}");
            text.AppendLine($"  Total Expense: {TotalExpense:C}");

            text.AppendLine("\nRevenue and Profit Data:");
            if (ChartData.Any())
            {
                foreach (var data in ChartData)
                {
                    text.AppendLine($"  {data.Label}: Revenue = {data.Revenue:C}, Profit = {data.Profit:C}");
                }
            }
            else
            {
                text.AppendLine("  No revenue or profit data available.");
            }

            text.AppendLine("\nExpense by Category:");
            if (ExpenseCategoryData.Any())
            {
                foreach (var category in ExpenseCategoryData)
                {
                    text.AppendLine($"  {category.CategoryName}: {category.Amount:C}");
                }
            }
            else
            {
                text.AppendLine("  No expense data available.");
            }

            return text.ToString();
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