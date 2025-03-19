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
            //GridContent.DataContext = IncomeViewModel;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ExpenseModel selectedExpense)
            {
                int id = selectedExpense.ExpenseCategoryId; //Category nha 
                Debug.WriteLine($"Selected Expense ID: {id}");
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
