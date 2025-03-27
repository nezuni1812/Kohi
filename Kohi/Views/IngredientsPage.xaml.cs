using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Kohi.Models;
using Kohi.ViewModels;
using System.Diagnostics;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IngredientsPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public IngredientsPage()
        {
            this.InitializeComponent();
            Loaded += IngredientsPage_Loaded;
        }

        public async void IngredientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await IngredientViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is IngredientModel selectedIngredient)
            {
                int id = selectedIngredient.Id;
                Debug.WriteLine($"Selected Expense ID: {id}");
            }
        }
        private void addButton_click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(AddNewIngredientPage), null);
        }

        public void UpdatePageList()
        {
            if (IngredientViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, IngredientViewModel.TotalPages);
            pageList.SelectedItem = IngredientViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IngredientViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != IngredientViewModel.CurrentPage)
            {
                await IngredientViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
    }
}
