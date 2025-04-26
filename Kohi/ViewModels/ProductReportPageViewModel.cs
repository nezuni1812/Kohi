using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoGen.Gemini;
using Kohi.Services;
using Kohi.Utils;
using AutoGen.Core;
using Syncfusion.UI.Xaml.Chat;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Text;

namespace Kohi.ViewModels
{
    internal class ProductReportPageViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private ObservableCollection<object> chats;
        private bool isTrue = false;
        private TypingIndicator typingIndicator;
        private IEnumerable<string> suggestion;
        private Author currentUser;
        static string apiKey = AppConfig.Configuration["GeminiAPIKey"];

        MiddlewareStreamingAgent<GeminiChatAgent> geminiAgent = new GeminiChatAgent(
            name: "gemini",
            model: "gemini-2.0-flash",
            apiKey: apiKey,
            systemMessage: "You are a helpful assistant for a POS coffee shop application for Vietnamese so most currency data will be in VNĐ." +
            " There will be a text version of what the user is seeing on the screen included in the prompt, please make use of it as much as you can. "+
            "Please noted if the user ask for something other than coffeeshop related like creating a programe with any sort of language, decline. If you are sure that the question is out of the scope of the provided data, you can just say you dont know")
        .RegisterMessageConnector()
        .RegisterPrintMessage();

        public ObservableCollection<object> Chats
        {
            get => this.chats;
            set
            {
                this.chats = value;
                RaisePropertyChanged("Messages");
            }
        }

        public bool IsTrue
        {
            get => this.isTrue;
            set
            {
                this.isTrue = value;
                RaisePropertyChanged("IsTrue");
            }
        }

        public TypingIndicator TypingIndicator
        {
            get => this.typingIndicator;
            set
            {
                this.typingIndicator = value;
                RaisePropertyChanged("TypingIndicator");
            }
        }

        public IEnumerable<string> Suggestion
        {
            get => this.suggestion;
            set
            {
                this.suggestion = value;
                RaisePropertyChanged("Suggestion");
            }
        }

        public Author CurrentUser
        {
            get => this.currentUser;
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }

        public class Model
        {
            public DateTime XValue { get; set; }
            public double YValue { get; set; }
        }

        public class TopProductModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public double TotalQuantity { get; set; }
            public double TotalRevenue { get; set; }
            public double Percentage { get; set; }
        }

        public ObservableCollection<Model> Data { get; set; }
        public ObservableCollection<Model> OutboundData { get; set; }
        public ObservableCollection<string> IngredientNames { get; set; }
        public ObservableCollection<TopProductModel> TopProducts { get; set; }

        private string _selectedIngredientName;
        public string SelectedIngredientName
        {
            get => _selectedIngredientName;
            set
            {
                _selectedIngredientName = value;
                OnPropertyChanged(nameof(SelectedIngredientName));
                LoadInboundDataAsync(_selectedIngredientName);
                LoadOutboundDataAsync(_selectedIngredientName);
            }
        }

        private string _selectedTimeRange;
        public string SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                _selectedTimeRange = value;
                OnPropertyChanged(nameof(SelectedTimeRange));
                UpdateDateRange();
                LoadDataAsync();
            }
        }

        private DateTimeOffset? _startDate;
        public DateTimeOffset? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTimeOffset? _endDate;
        public DateTimeOffset? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ProductReportPageViewModel()
        {
            _dao = Services.Service.GetKeyedSingleton<IDao>();
            Data = new ObservableCollection<Model>();
            OutboundData = new ObservableCollection<Model>();
            IngredientNames = new ObservableCollection<string>();
            TopProducts = new ObservableCollection<TopProductModel>();

            this.Chats = new ObservableCollection<object>();
            Suggestion = new ObservableCollection<string>
            {
                "Xác định các vấn đề tiềm ẩn dựa trên xu hướng nhập/xuất kho",
                "Sản phẩm bán chạy nhất là gì?",
                "Cho những vấn đề tôi cần quan tâm",
                "Lịch sử nhập kho và xuất kho có ổn không"
            };

            this.CurrentUser = new Author { Name = "User" };
            this.TypingIndicator = new TypingIndicator { Author = new Author { Name = "Bot" } };
            this.Chats.CollectionChanged += Chats_CollectionChanged;

            if (apiKey is null)
            {
                Console.WriteLine("Please set GOOGLE_GEMINI_API_KEY environment variable.");
                return;
            }

            SelectedTimeRange = "Hôm nay";
            //LoadDataAsync();
            Debug.WriteLine("Added data");
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

        private async void Chats_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems?[0] as ITextMessage;
            if (item != null && item.Author.Name == CurrentUser.Name)
            {
                Debug.WriteLine("user text: " + item.Text);
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

        private void UpdateDateRange()
        {
            var today = DateTimeOffset.Now.Date;
            switch (SelectedTimeRange)
            {
                case "Hôm nay":
                    StartDate = today;
                    EndDate = today.AddDays(1);
                    break;
                case "Tuần này":
                    StartDate = today.AddDays(-(int)today.DayOfWeek);
                    EndDate = StartDate.Value.AddDays(7);
                    break;
                case "Tháng này":
                    StartDate = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    EndDate = StartDate.Value.AddMonths(1);
                    break;
                case "Năm này":
                    StartDate = new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    EndDate = StartDate.Value.AddYears(1);
                    break;
                case "Tùy chỉnh":
                    break;
            }
        }

        private async Task<string> GetChartDataAsText()
        {
            var text = new StringBuilder();

            if (StartDate.HasValue && EndDate.HasValue)
            {
                text.AppendLine($"Data Range: {StartDate.Value:yyyy-MM-dd} to {EndDate.Value:yyyy-MM-dd}");
            }
            else
            {
                text.AppendLine("Data Range: All time");
            }

            // inbound byingredient
            var allInboundsByIngredient = new Dictionary<string, List<(DateTime Date, double Quantity)>>();
            const int pageSize = 100;
            int currentPage = 1;
            int totalItems = _dao.Inbounds.GetCount();

            while ((currentPage - 1) * pageSize < totalItems)
            {
                var inbounds = await Task.Run(() => _dao.Inbounds.GetAll(
                    pageNumber: currentPage,
                    pageSize: pageSize
                ));

                if (inbounds == null || !inbounds.Any())
                {
                    break;
                }

                foreach (var inbound in inbounds)
                {
                    if (StartDate.HasValue && EndDate.HasValue &&
                        (inbound.InboundDate.Date < StartDate.Value.Date || inbound.InboundDate.Date > EndDate.Value.Date))
                    {
                        continue;
                    }

                    var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                    if (ingredient != null && !string.IsNullOrEmpty(ingredient.Name))
                    {
                        if (!allInboundsByIngredient.ContainsKey(ingredient.Name))
                        {
                            allInboundsByIngredient[ingredient.Name] = new List<(DateTime, double)>();
                        }
                        allInboundsByIngredient[ingredient.Name].Add((inbound.InboundDate, inbound.Quantity));
                    }
                }

                currentPage++;
            }

            // outbound ingredient
            var allOutboundsByIngredient = new Dictionary<string, List<(DateTime Date, double Quantity)>>();
            currentPage = 1;
            totalItems = _dao.Outbounds.GetCount();

            while ((currentPage - 1) * pageSize < totalItems)
            {
                var outbounds = await Task.Run(() => _dao.Outbounds.GetAll(
                    pageNumber: currentPage,
                    pageSize: pageSize
                ));

                if (outbounds == null || !outbounds.Any())
                {
                    break;
                }

                foreach (var outbound in outbounds)
                {
                    if (StartDate.HasValue && EndDate.HasValue &&
                        (outbound.OutboundDate.Date < StartDate.Value.Date || outbound.OutboundDate.Date > EndDate.Value.Date))
                    {
                        continue;
                    }

                    var inventory = await Task.Run(() => _dao.Inventories.GetById(outbound.InventoryId + ""));
                    if (inventory == null) continue;

                    var inbound = await Task.Run(() => _dao.Inbounds.GetById(inventory.InboundId + ""));
                    if (inbound == null) continue;

                    var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                    if (ingredient != null && !string.IsNullOrEmpty(ingredient.Name))
                    {
                        if (!allOutboundsByIngredient.ContainsKey(ingredient.Name))
                        {
                            allOutboundsByIngredient[ingredient.Name] = new List<(DateTime, double)>();
                        }
                        allOutboundsByIngredient[ingredient.Name].Add((outbound.OutboundDate, (double)outbound.Quantity));
                    }
                }

                currentPage++;
            }

            var allIngredients = allInboundsByIngredient.Keys.Union(allOutboundsByIngredient.Keys).OrderBy(k => k).ToList();

            foreach (var ingredient in allIngredients)
            {
                text.AppendLine($"\nIngredient: {ingredient}");

                text.AppendLine("  Inbound Data:");
                if (allInboundsByIngredient.ContainsKey(ingredient) && allInboundsByIngredient[ingredient].Any())
                {
                    var aggregatedInbounds = allInboundsByIngredient[ingredient]
                        .GroupBy(i => i.Date.Date)
                        .Select(g => new { Date = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                        .OrderBy(x => x.Date)
                        .ToList();

                    foreach (var item in aggregatedInbounds)
                    {
                        text.AppendLine($"    {item.Date:yyyy-MM-dd}: {item.TotalQuantity}");
                    }
                }
                else
                {
                    text.AppendLine("    No inbound data available.");
                }

                text.AppendLine("  Outbound Data:");
                if (allOutboundsByIngredient.ContainsKey(ingredient) && allOutboundsByIngredient[ingredient].Any())
                {
                    var aggregatedOutbounds = allOutboundsByIngredient[ingredient]
                        .GroupBy(i => i.Date.Date)
                        .Select(g => new { Date = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                        .OrderBy(x => x.Date)
                        .ToList();

                    foreach (var item in aggregatedOutbounds)
                    {
                        text.AppendLine($"    {item.Date:yyyy-MM-dd}: {item.TotalQuantity}");
                    }
                }
                else
                {
                    text.AppendLine("    No outbound data available.");
                }
            }

            if (!string.IsNullOrEmpty(SelectedIngredientName))
            {
                text.AppendLine($"\nCurrently Selected Ingredient: {SelectedIngredientName}");
            }
            else
            {
                text.AppendLine("\nNo ingredient currently selected.");
            }

            text.AppendLine("\nTop 10 Most Popular Products:");
            if (TopProducts.Any())
            {
                foreach (var product in TopProducts)
                {
                    text.AppendLine($"  {product.Name}: {product.TotalQuantity} units, Revenue: {product.TotalRevenue:C}, Percentage: {product.Percentage:F2}%");
                }
            }
            else
            {
                text.AppendLine("  No product data available.");
            }

            return text.ToString();
        }

        public async Task LoadDataAsync()
        {
            await LoadIngredientNamesAsync();
            await LoadInboundDataAsync();
            await LoadOutboundDataAsync();
            await LoadTopProductsAsync();
        }

        private async Task LoadInboundDataAsync(string ingredientName = null)
        {
            try
            {
                Data.Clear();
                const int pageSize = 100;
                int currentPage = 1;
                int totalItems = _dao.Inbounds.GetCount();
                var allInbounds = new List<(DateTime InboundDate, double Quantity)>();

                while ((currentPage - 1) * pageSize < totalItems)
                {
                    var inbounds = await Task.Run(() => _dao.Inbounds.GetAll(
                        pageNumber: currentPage,
                        pageSize: pageSize
                    ));

                    if (inbounds == null || !inbounds.Any())
                    {
                        Debug.WriteLine($"No inbound data found for page {currentPage}.");
                        break;
                    }

                    foreach (var inbound in inbounds)
                    {
                        if (StartDate.HasValue && EndDate.HasValue &&
                            (inbound.InboundDate.Date < StartDate.Value.Date || inbound.InboundDate.Date > EndDate.Value.Date))
                        {
                            continue;
                        }

                        var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                        if (ingredientName == null || ingredient?.Name == ingredientName)
                        {
                            allInbounds.Add((inbound.InboundDate, inbound.Quantity));
                        }
                    }

                    currentPage++;
                }

                if (allInbounds.Any())
                {
                    var aggregatedData = allInbounds
                        .GroupBy(i => i.InboundDate.Date)
                        .Select(g => new
                        {
                            InboundDate = g.Key,
                            TotalQuantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.InboundDate)
                        .ToList();

                    foreach (var item in aggregatedData)
                    {
                        Data.Add(new Model
                        {
                            XValue = item.InboundDate,
                            YValue = item.TotalQuantity
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("No inbound data found.");
                    Data.Add(new Model { XValue = DateTime.Now, YValue = 0 });
                }

                Debug.WriteLine($"Loaded {Data.Count} inbound records.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading inbound data: {ex.Message}");
                Data.Add(new Model { XValue = DateTime.Now, YValue = 0 });
            }

            OnPropertyChanged(nameof(Data));
        }

        private async Task LoadOutboundDataAsync(string ingredientName = null)
        {
            try
            {
                OutboundData.Clear();
                const int pageSize = 100;
                int currentPage = 1;
                int totalItems = _dao.Outbounds.GetCount();
                var allOutbounds = new List<(DateTime OutboundDate, double Quantity)>();

                while ((currentPage - 1) * pageSize < totalItems)
                {
                    var outbounds = await Task.Run(() => _dao.Outbounds.GetAll(
                        pageNumber: currentPage,
                        pageSize: pageSize
                    ));

                    if (outbounds == null || !outbounds.Any())
                    {
                        Debug.WriteLine($"No outbound data found for page {currentPage}.");
                        break;
                    }

                    foreach (var outbound in outbounds)
                    {
                        if (StartDate.HasValue && EndDate.HasValue &&
                            (outbound.OutboundDate.Date < StartDate.Value.Date || outbound.OutboundDate.Date > EndDate.Value.Date))
                        {
                            continue;
                        }

                        var inventory = await Task.Run(() => _dao.Inventories.GetById(outbound.InventoryId + ""));
                        if (inventory == null)
                        {
                            continue;
                        }

                        var inbound = await Task.Run(() => _dao.Inbounds.GetById(inventory.InboundId + ""));
                        if (inbound == null)
                        {
                            continue;
                        }

                        var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                        if (ingredientName == null || ingredient?.Name == ingredientName)
                        {
                            allOutbounds.Add((outbound.OutboundDate, (double)outbound.Quantity));
                        }
                    }

                    currentPage++;
                }

                if (allOutbounds.Any())
                {
                    var aggregatedData = allOutbounds
                        .GroupBy(i => i.OutboundDate.Date)
                        .Select(g => new
                        {
                            OutboundDate = g.Key,
                            TotalQuantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.OutboundDate)
                        .ToList();

                    foreach (var item in aggregatedData)
                    {
                        OutboundData.Add(new Model
                        {
                            XValue = item.OutboundDate,
                            YValue = item.TotalQuantity
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("No outbound data found.");
                    OutboundData.Add(new Model { XValue = DateTime.Now, YValue = 0 });
                }

                Debug.WriteLine($"Loaded {OutboundData.Count} outbound records.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading outbound data: {ex.Message}");
                OutboundData.Add(new Model { XValue = DateTime.Now, YValue = 0 });
            }

            OnPropertyChanged(nameof(OutboundData));
        }

        private async Task LoadIngredientNamesAsync()
        {
            try
            {
                const int pageSize = 100;
                int currentPage = 1;
                int totalItems = _dao.Ingredients.GetCount();
                var allNames = new List<string>();

                while ((currentPage - 1) * pageSize < totalItems)
                {
                    var ingredients = await Task.Run(() => _dao.Ingredients.GetAll(
                        pageNumber: currentPage,
                        pageSize: pageSize
                    ));

                    if (ingredients == null || !ingredients.Any())
                    {
                        Debug.WriteLine($"No ingredients found for page {currentPage}.");
                        break;
                    }

                    foreach (var ingredient in ingredients)
                    {
                        if (!string.IsNullOrEmpty(ingredient.Name))
                        {
                            allNames.Add(ingredient.Name);
                        }
                    }

                    currentPage++;
                }

                if (allNames.Any())
                {
                    foreach (var name in allNames.OrderBy(n => n))
                    {
                        IngredientNames.Add(name);
                    }
                }
                else
                {
                    Debug.WriteLine("No ingredient names found.");
                    IngredientNames.Add("No Ingredients");
                }

                Debug.WriteLine($"Loaded {IngredientNames.Count} ingredient names.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading ingredient names: {ex.Message}");
                IngredientNames.Add("Error Loading Ingredients");
            }

            OnPropertyChanged(nameof(IngredientNames));
        }

        private async Task LoadTopProductsAsync()
        {
            try
            {
                TopProducts.Clear();
                const int pageSize = 1000;
                int currentPage = 1;
                //int totalItems = _dao.InvoiceDetails.GetCount();
                var productSales = new List<(int Id, string Name, double Quantity, double Revenue)>();

                //while ((currentPage - 1) * pageSize < totalItems)
                //{
                var invoiceDetails = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                    pageNumber: currentPage,
                    pageSize: pageSize
                ));

                //if (invoiceDetails == null || !invoiceDetails.Any())
                //{
                //    Debug.WriteLine($"No invoice details found for page {currentPage}.");
                //    break;
                //}

                foreach (var detail in invoiceDetails)
                {
                    var invoice = await Task.Run(() => _dao.Invoices.GetById(detail.InvoiceId + ""));
                    if (invoice == null)
                    {
                        continue;
                    }

                    if (StartDate.HasValue && EndDate.HasValue &&
                        (invoice.InvoiceDate.Date < StartDate.Value.Date || invoice.InvoiceDate.Date > EndDate.Value.Date))
                    {
                        continue;
                    }

                    var productVariant = await Task.Run(() => _dao.ProductVariants.GetById(detail.ProductId + ""));
                    if (productVariant == null)
                    {
                        Debug.WriteLine($"No product variant found for ID {detail.ProductId}");
                        continue;
                    }

                    var product = await Task.Run(() => _dao.Products.GetById(productVariant.ProductId + ""));
                    if (product == null)
                    {
                        Debug.WriteLine($"No product found for ID {productVariant.ProductId}");
                        continue;
                    }

                    var revenue = detail.Quantity * productVariant.Price;
                    Debug.WriteLine("Add: " + product.Id + " " + product.Name);
                    productSales.Add((product.Id, product.Name, detail.Quantity, revenue));
                }

                currentPage++;
                //}

                if (productSales.Any())
                {
                    var aggregatedSales = productSales
                        .GroupBy(p => new { p.Id, p.Name })
                        .Select(g => new
                        {
                            Id = g.Key.Id,
                            Name = g.Key.Name,
                            TotalQuantity = g.Sum(x => x.Quantity),
                            TotalRevenue = g.Sum(x => x.Revenue)
                        })
                        .OrderByDescending(x => x.TotalQuantity)
                        .Take(10)
                        .ToList();

                    var totalQuantitySum = aggregatedSales.Sum(x => x.TotalQuantity);

                    foreach (var item in aggregatedSales)
                    {
                        Debug.WriteLine("Aggregated product: " + item.Id);
                        var percentage = totalQuantitySum > 0 ? (item.TotalQuantity / totalQuantitySum) * 100 : 0;
                        TopProducts.Add(new TopProductModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            TotalQuantity = item.TotalQuantity,
                            TotalRevenue = item.TotalRevenue,
                            Percentage = percentage
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("No product sales data found.");
                    TopProducts.Add(new TopProductModel
                    {
                        Id = 0,
                        Name = "No Data",
                        TotalQuantity = 0,
                        TotalRevenue = 0,
                        Percentage = 0
                    });
                }

                Debug.WriteLine($"Loaded {TopProducts.Count} top products.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading top products: {ex.Message}");
                TopProducts.Add(new TopProductModel
                {
                    Id = 0,
                    Name = "Error Loading Data",
                    TotalQuantity = 0,
                    TotalRevenue = 0,
                    Percentage = 0
                });
            }

            Debug.WriteLine("End of load");
            OnPropertyChanged(nameof(TopProducts));
        }
    }
}
