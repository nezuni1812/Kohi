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
using Google.Api;
using Microsoft.UI.Xaml.Controls;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Collections.Generic;

namespace Kohi.ViewModels
{
    internal class ProductReportPageViewModel : INotifyPropertyChanged
    {
        private IDao _dao;

        private ObservableCollection<object> chats;
        private IEnumerable<string> suggestion;
        public IEnumerable<string> Suggestion
        {
            get => this.suggestion;
            set
            {
                this.suggestion = value;
                RaisePropertyChanged("Suggestion");
            }
        }

        private Author currentUser;
        static string apiKey = AppConfig.Configuration["GeminiAPIKey"];

        MiddlewareStreamingAgent<GeminiChatAgent> geminiAgent = new GeminiChatAgent(
            name: "gemini",
            model: "gemini-2.0-flash",
            apiKey: apiKey,
            systemMessage: "You are a helpful assistant for a POS coffee shop application. There will be a text version of what the user is seeing on the screen included in the prompt, please make use of it as much as you can")
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

        public Author CurrentUser
        {
            get => this.currentUser;
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public class Model
        {
            public DateTime XValue { get; set; }
            public double YValue { get; set; }
        }

        public ObservableCollection<Model> Data { get; set; } // Inbound data
        public ObservableCollection<Model> OutboundData { get; set; } // Outbound data
        public ObservableCollection<string> IngredientNames { get; set; }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductReportPageViewModel()
        {
            _dao = Services.Service.GetKeyedSingleton<IDao>();
            Data = new ObservableCollection<Model>();
            OutboundData = new ObservableCollection<Model>();
            IngredientNames = new ObservableCollection<string>();

            var expenses = _dao.Expenses.GetAll(filterField: "amount", filterValue: "2000000");
            Debug.WriteLine("Amount of expenses: " + expenses.Count);
            foreach (var expense in expenses)
                Debug.WriteLine($"Expense: {expense.Amount}");

            LoadDataAsync();
            Debug.WriteLine("Added data");

            this.Chats = new ObservableCollection<object>();
            Suggestion = new ObservableCollection<string> { "Identify potential issues based on inbound/outbound trends", "Other insights?", "Cho những vấn đề tôi cần quan tâm", "Lịch sử nhập kho và xuất kho có ổn không" };

            this.CurrentUser = new Author { Name = "User" };
            this.Chats.CollectionChanged += Chats_CollectionChanged;

            if (apiKey is null)
            {
                Console.WriteLine("Please set GOOGLE_GEMINI_API_KEY environment variable.");
                return;
            }
        }

        public void HandleSuggestionClicked(string suggestionText)
        {
            // Add the suggestion as a user message to the Chats collection
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
            if (item != null)
            {
                if (item.Author.Name == CurrentUser.Name)
                {
                    Debug.WriteLine("user text: " + item.Text);
                    string Response = string.Empty;
                    Debug.WriteLine("Generating...");

                    string chartDataText = await GetChartDataAsText();
                    string messageWithData = $"{item.Text}\n\nHere is the chart data the user is seeing, please answer according to the data in it:\n{chartDataText}";
                    var reply = await geminiAgent.SendAsync(messageWithData);
                    Debug.WriteLine("Response: " + reply.GetContent());
                    Response = reply.GetContent();

                    Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage
                    {
                        Author = new Author { Name = "Bot" },
                        DateTime = DateTime.Now,
                        Text = Response
                    });
                }
            }
        }

        private async Task<string> GetChartDataAsText()
        {
            var text = new System.Text.StringBuilder();

            // Fetch all inbound data grouped by ingredient
            var allInboundsByIngredient = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(DateTime Date, double Quantity)>>();
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
                    var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                    if (ingredient != null && !string.IsNullOrEmpty(ingredient.Name))
                    {
                        if (!allInboundsByIngredient.ContainsKey(ingredient.Name))
                        {
                            allInboundsByIngredient[ingredient.Name] = new System.Collections.Generic.List<(DateTime, double)>();
                        }
                        allInboundsByIngredient[ingredient.Name].Add((inbound.InboundDate, inbound.Quantity));
                    }
                }

                currentPage++;
            }

            // Fetch all outbound data grouped by ingredient
            var allOutboundsByIngredient = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(DateTime Date, double Quantity)>>();
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
                    var inventory = await Task.Run(() => _dao.Inventories.GetById(outbound.InventoryId + ""));
                    if (inventory == null) continue;

                    var inbound = await Task.Run(() => _dao.Inbounds.GetById(inventory.InboundId + ""));
                    if (inbound == null) continue;

                    var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                    if (ingredient != null && !string.IsNullOrEmpty(ingredient.Name))
                    {
                        if (!allOutboundsByIngredient.ContainsKey(ingredient.Name))
                        {
                            allOutboundsByIngredient[ingredient.Name] = new System.Collections.Generic.List<(DateTime, double)>();
                        }
                        allOutboundsByIngredient[ingredient.Name].Add((outbound.OutboundDate, (double)outbound.Quantity));
                    }
                }

                currentPage++;
            }

            // Combine all ingredients
            var allIngredients = allInboundsByIngredient.Keys.Union(allOutboundsByIngredient.Keys).OrderBy(k => k).ToList();

            // Format the data for each ingredient
            foreach (var ingredient in allIngredients)
            {
                text.AppendLine($"\nIngredient: {ingredient}");

                // Inbound data for this ingredient
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

                // Outbound data for this ingredient
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

            // Add selected ingredient if applicable
            if (!string.IsNullOrEmpty(SelectedIngredientName))
            {
                text.AppendLine($"\nCurrently Selected Ingredient: {SelectedIngredientName}");
            }
            else
            {
                text.AppendLine("\nNo ingredient currently selected.");
            }

            return text.ToString();
        }

        private async Task LoadDataAsync()
        {
            await LoadIngredientNamesAsync();
            await LoadInboundDataAsync();
            await LoadOutboundDataAsync();
        }

        private async Task LoadInboundDataAsync(string ingredientName = null)
        {
            try
            {
                Data.Clear();
                const int pageSize = 100;
                int currentPage = 1;
                int totalItems = _dao.Inbounds.GetCount();
                var allInbounds = new System.Collections.Generic.List<(DateTime InboundDate, double Quantity)>();

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
                        var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                        if (ingredientName == null || ingredient?.Name == ingredientName)
                        {
                            Debug.WriteLine($"{inbound.InboundDate}: {inbound.Quantity}");
                            allInbounds.Add((inbound.InboundDate, inbound.Quantity));
                        }
                    }

                    currentPage++;
                }

                if (allInbounds.Any())
                {
                    var aggregatedData = allInbounds
                        .GroupBy(i => i.InboundDate.Date) // Group by date (ignoring time)
                        .Select(g => new
                        {
                            InboundDate = g.Key,
                            TotalQuantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.InboundDate)
                        .ToList();

                    foreach (var item in aggregatedData)
                    {
                        Debug.WriteLine($"Aggregated Inbound: {item.InboundDate}: {item.TotalQuantity}");
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
                var allOutbounds = new System.Collections.Generic.List<(DateTime OutboundDate, double Quantity)>();

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
                        var inventory = await Task.Run(() => _dao.Inventories.GetById(outbound.InventoryId + ""));
                        if (inventory == null)
                        {
                            Debug.WriteLine($"No inventory found for outbound ID {outbound.Id}");
                            continue;
                        }

                        var inbound = await Task.Run(() => _dao.Inbounds.GetById(inventory.InboundId + ""));
                        if (inbound == null)
                        {
                            Debug.WriteLine($"No inbound found for inventory ID {inventory.Id}");
                            continue;
                        }

                        var ingredient = await Task.Run(() => _dao.Ingredients.GetById(inbound.IngredientId + ""));
                        if (ingredientName == null || ingredient?.Name == ingredientName)
                        {
                            Debug.WriteLine($"{outbound.OutboundDate}: {outbound.Quantity}");
                            allOutbounds.Add((outbound.OutboundDate, (double)outbound.Quantity));
                        }
                    }

                    currentPage++;
                }

                if (allOutbounds.Any())
                {
                    var aggregatedData = allOutbounds
                        .GroupBy(i => i.OutboundDate.Date) // Group by date (ignoring time)
                        .Select(g => new
                        {
                            OutboundDate = g.Key,
                            TotalQuantity = g.Sum(x => x.Quantity)
                        })
                .OrderBy(x => x.OutboundDate)
                        .ToList();

                    foreach (var item in aggregatedData)
                    {
                        Debug.WriteLine($"Aggregated Outbound: {item.OutboundDate}: {item.TotalQuantity}");
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
                var allNames = new System.Collections.Generic.List<string>();

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
    }
}