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
using System.Collections.ObjectModel;
using Kohi.Utils;
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
        public ExpenseCategoryViewModel ExpenseCategoryViewModel { get; set; } = new ExpenseCategoryViewModel();
        public ExpenseModel SelectedExpense { get; set; }

        public IncomeExpensePage()
        {
            this.InitializeComponent();
            Loaded += ExpensesPage_Loaded;
            this.DataContext = this;

            //GridContent.DataContext = IncomeViewModel;
        }

        public async void ExpensesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ExpenseViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ExpenseModel selectedExpense)
            {
                SelectedExpense = selectedExpense;
                Debug.WriteLine($"Selected Expense ID: {SelectedExpense.Id}");
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {

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
            //if (sender.SelectedItem == IncomeSelectorBar)
            //{
            //    //ReceiptCategory.Text = "Loại phiếu thu";
            //    //Amount.Text = "Số tiền thu";
            //    addButtonTextBlock.Text = "Thêm phiếu thu";


            //    //GridContent.DataContext = IncomeViewModel;
            //}
            //else if (sender.SelectedItem == ExpenseSelectorBar)
            //{
            //    //ReceiptCategory.Text = "Loại phiếu chi";
            //    //Amount.Text = "Số tiền chi";
            //    addButtonTextBlock.Text = "Thêm phiếu chi";
            //    MyTableView.ItemsSource = ExpenseViewModel.ExpenseReceipts; 
            //    //GridContent.DataContext = ExpenseViewModel;
            //}
        }
        private void AddExpenseReceiptDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Simply close the dialog without saving
        }
        private void EditExpenseReceiptDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Simply close the dialog without saving
        }

        public async void showDeleteExpenseReceiptDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedExpense == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có phiếu nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa phiếu chi này không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Actually delete the expense receipt
                await ExpenseViewModel.Delete(SelectedExpense.Id.ToString());
                Debug.WriteLine($"Đã xóa phiếu chi ID: {SelectedExpense.Id}");
            }
            else
            {
                Debug.WriteLine("Hủy xóa phiếu");
            }
        }

        public async void showEditExpenseReceiptDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedExpense == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có phiếu nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            var selectedExpenseCategory = SelectedExpense.ExpenseCategory;

            Debug.WriteLine($"Editing expense receipt ID: {SelectedExpense.Id}");
            EditExpenseReceiptCategoryComboBox.SelectedItem = selectedExpenseCategory;
            EditExpenseReceiptAmount.Text = SelectedExpense.Amount.ToString();
            EditExpenseReceiptDate.Date = SelectedExpense.ExpenseDate;
            EditExpenseReceiptNote.Text = SelectedExpense.Note;
            var result = await EditExpenseReceiptDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Validate input
                if (EditExpenseReceiptCategoryComboBox.SelectedItem == null)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng chọn loại phiếu chi",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditExpenseReceiptAmount.Text) ||
                    !decimal.TryParse(EditExpenseReceiptAmount.Text, out decimal amount))
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng nhập số tiền hợp lệ",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                if (EditExpenseReceiptDate.Date == null)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng chọn ngày",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }
                selectedExpenseCategory = EditExpenseReceiptCategoryComboBox.SelectedItem as ExpenseCategoryModel;
                // Update the expense model
                SelectedExpense.ExpenseCategoryId = selectedExpenseCategory.Id;
                SelectedExpense.Amount = float.Parse(EditExpenseReceiptAmount.Text);
                SelectedExpense.ExpenseDate = EditExpenseReceiptDate.Date.Value.DateTime;
                SelectedExpense.Note = EditExpenseReceiptNote.Text;
                // Save changes
                await ExpenseViewModel.Update(SelectedExpense.Id.ToString(), SelectedExpense);
                // Refresh data
                await ExpenseViewModel.LoadData();
                UpdatePageList();
            }
        }

        public async void showAddExpenseReceiptDialog_Click(object sender, RoutedEventArgs e)
        {

            // Clear form fields
            AddExpenseReceiptCategoryComboBox.SelectedItem = null;
            AddExpenseReceiptAmount.Text = string.Empty;
            AddExpenseReceiptDate.Date = DateTime.Today; // Set to current date
            AddExpenseReceiptNote.Text = string.Empty;

            var result = await AddExpenseReceiptDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Validate input
                if (AddExpenseReceiptCategoryComboBox.SelectedItem == null)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng chọn loại phiếu chi",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                if (string.IsNullOrWhiteSpace(AddExpenseReceiptAmount.Text) ||
                    !decimal.TryParse(AddExpenseReceiptAmount.Text, out decimal amount))
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng nhập số tiền hợp lệ",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                if (AddExpenseReceiptDate.Date == null)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng chọn ngày",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }
                var selectedExpenseCategory = AddExpenseReceiptCategoryComboBox.SelectedItem as ExpenseCategoryModel;
                // Create new expense
                var newExpense = new ExpenseModel
                {
                    ExpenseCategoryId = selectedExpenseCategory.Id,
                    Amount = float.Parse(AddExpenseReceiptAmount.Text),
                    ExpenseDate = AddExpenseReceiptDate.Date.Value.DateTime,
                    Note = AddExpenseReceiptNote.Text
                };

                // Add to database
                await ExpenseViewModel.Add(newExpense);
            }
        }
    }
}
