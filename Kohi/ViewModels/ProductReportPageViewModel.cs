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

namespace Kohi.ViewModels
{
    internal class ProductReportPageViewModel : INotifyPropertyChanged
    {
        private IDao _dao;

        private ObservableCollection<object> chats;
        private Author currentUser;

        //string apiKey = Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY");
        static string apiKey = "AIzaSyD39bsb7FdniVP0yvGB83tPhnFtSYiwusw";

        MiddlewareStreamingAgent<GeminiChatAgent> geminiAgent = new GeminiChatAgent(
            name: "gemini",
            model: "gemini-1.5-flash-001",
            apiKey: apiKey,
            systemMessage: "You are a helpful assisstant for a POS coffee shop Application, if the user asked for something you have no knowlege nor data of, please just straight out say you don't know")
        .RegisterMessageConnector()
        .RegisterPrintMessage();

        private async void GenerateMessages()
        {
            if (apiKey is null)
            {
                Console.WriteLine("Please set GOOGLE_GEMINI_API_KEY environment variable.");
                return;
            }
            //var reply = await geminiAgent.SendAsync("Can you write a piece of C# code to calculate 100th of fibonacci?");
            //Debug.WriteLine(reply);

            //this.Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage { Author = CurrentUser, Text = "What is WinUI?" });
            //await Task.Delay(1000);
            //this.Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage { Author = new Author { Name = "Bot" }, Text = "WinUI is a user interface layer that contains modern controls and styles for building Windows apps." });
        }

        public ObservableCollection<object> Chats
        {
            get
            {
                return this.chats;
            }
            set
            {
                this.chats = value;
                RaisePropertyChanged("Messages");
            }
        }

        public Author CurrentUser
        {
            get
            {
                return this.currentUser;
            }
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }


        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
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

            LoadDataAsync();
            Debug.WriteLine("Added data");

            this.Chats = new ObservableCollection<object>();
            this.CurrentUser = new Author { Name = "John" };
            this.Chats.CollectionChanged += Chats_CollectionChanged;
            this.GenerateMessages();
        }

        private async void Chats_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems?[0] as ITextMessage;
            if (item != null)
            {
                if (item.Author.Name == currentUser.Name)
                {
                    Debug.WriteLine("user text: " + item.Text);
                    //ShowTypingIndicator = true;
                    string Response = string.Empty;
                    //var response = await gpt.GetChatMessageContentAsync(line);
                    Debug.WriteLine("Generating...");
                    var reply = await geminiAgent.SendAsync(item.Text);
                    Debug.WriteLine("Response: " + reply.GetContent());
                    Response = reply.GetContent();

                    //await service.NonStreamingChat(item.Text);
                    Chats.Add(new Syncfusion.UI.Xaml.Chat.TextMessage
                    {
                        Author = new Author { Name = "Bot" },
                        DateTime = DateTime.Now,
                        Text = Response
                    });
                    //ShowTypingIndicator = false;
                }
            }
        }

        private async Task LoadDataAsync()
        {
            LoadIngredientNamesAsync();
            LoadInboundDataAsync();
            LoadOutboundDataAsync();
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