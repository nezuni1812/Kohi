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
using Kohi.ViewModels;
using WinUI.TableView;
using Kohi.Models;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IncomeExpensePage : Page
    {
        public IncomeViewModel IncomeViewModel { get; set; } = new IncomeViewModel();
        public ExpenseViewModel ExpenseViewModel { get; set; } = new ExpenseViewModel();
        public IncomeExpensePage()
        {
            this.InitializeComponent();
            Loaded += ExpensesPage_Loaded;

            //GridContent.DataContext = IncomeViewModel;
        }

        public async void ExpensesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ExpenseViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is CustomerModel selectedCustomer)
            {
                int id = selectedCustomer.Id;
                Debug.WriteLine($"Selected Customer ID: {id}");
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {
            // Logic thêm khách hàng
        }

        public void UpdatePageList()
        {
            if (ExpenseViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, ExpenseViewModel.TotalPages);
            pageList.SelectedItem = ExpenseViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExpenseViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != ExpenseViewModel.CurrentPage)
            {
                await ExpenseViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == IncomeSelectorBar)
            {
                //ReceiptCategory.Text = "Loại phiếu thu";
                //Amount.Text = "Số tiền thu";
                addButtonTextBlock.Text = "Thêm phiếu thu";
                

                //GridContent.DataContext = IncomeViewModel;
            }
            else if (sender.SelectedItem == ExpenseSelectorBar)
            {
                //ReceiptCategory.Text = "Loại phiếu chi";
                //Amount.Text = "Số tiền chi";
                addButtonTextBlock.Text = "Thêm phiếu chi";
                MyTableView.ItemsSource = ExpenseViewModel.ExpenseReceipts; 
                //GridContent.DataContext = ExpenseViewModel;
            }
        }

        private void addButton_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
