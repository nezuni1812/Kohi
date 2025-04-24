using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Kohi.Services;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    internal class ProductReportPageViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        public class Model
        {
            public DateTime XValue { get; set; }
            public double YValue { get; set; }
        }

        public ObservableCollection<Model> Data { get; set; }
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
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductReportPageViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Data = new ObservableCollection<Model>();
            IngredientNames = new ObservableCollection<string>();

            LoadDataAsync();
            Debug.WriteLine("Added data");
        }

        private async Task LoadDataAsync()
        {
            LoadIngredientNamesAsync();
            LoadInboundDataAsync();
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
                        Debug.WriteLine($"Aggregated: {item.InboundDate}: {item.TotalQuantity}");
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