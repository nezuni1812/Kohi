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
using Kohi.Errors;
using System.Threading.Tasks;

namespace Kohi.Views
{
    public sealed partial class IncomeExpensePage : Page
    {
        public IncomeViewModel IncomeViewModel { get; set; } = new IncomeViewModel();
        public ExpenseViewModel ExpenseViewModel { get; set; } = new ExpenseViewModel();
        public ExpenseCategoryViewModel ExpenseCategoryViewModel { get; set; } = new ExpenseCategoryViewModel();
        public ExpenseModel SelectedExpense { get; set; }
        private readonly IErrorHandler _errorHandler;
        public bool IsLoading { get; set; } = false;

        public IncomeExpensePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            var emptyInputHandler = new EmptyInputErrorHandler();
            var positiveNumberHandler = new PositiveNumberValidationErrorHandler(new List<string>
            {
                "Số tiền"
            });
            emptyInputHandler.SetNext(positiveNumberHandler);
            _errorHandler = emptyInputHandler;
            Loaded += ExpensesPage_Loaded;
        }

        private async void ExpensesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress();
        }

        private async Task LoadDataWithProgress(int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;

                await Task.WhenAll(
                    ExpenseViewModel.LoadData(page),
                    ExpenseCategoryViewModel.LoadData()
                );
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading expense receipts: {ex.Message}");
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể tải dữ liệu: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
            }
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ExpenseModel selectedExpense)
            {
                SelectedExpense = selectedExpense;
                Debug.WriteLine($"Selected Expense ID: {SelectedExpense.Id}");
                editButton.IsEnabled = true;
                deleteButton.IsEnabled = true;
            }
            else
            {
                SelectedExpense = null;
                editButton.IsEnabled = false;
                deleteButton.IsEnabled = false;
            }
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
                await LoadDataWithProgress(selectedPage);
            }
        }

        private void AddExpenseReceiptDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void EditExpenseReceiptDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
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

            try
            {
                var result = await deleteDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ExpenseViewModel.Delete(SelectedExpense.Id.ToString());
                    Debug.WriteLine($"Đã xóa phiếu chi ID: {SelectedExpense.Id}");
                    await LoadDataWithProgress(ExpenseViewModel.CurrentPage);
                }
                else
                {
                    Debug.WriteLine("Hủy xóa phiếu");
                }
            }
            catch
            {

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

            await ExpenseCategoryViewModel.LoadData();
            var selectedExpenseCategory = ExpenseCategoryViewModel.ExpenseCategories.FirstOrDefault(c => c.Id == SelectedExpense.ExpenseCategoryId);

            Debug.WriteLine($"Editing expense receipt ID: {SelectedExpense.Id}");
            EditExpenseReceiptCategoryComboBox.SelectedItem = selectedExpenseCategory;
            EditExpenseReceiptAmount.Text = SelectedExpense.Amount.ToString();
            EditExpenseReceiptDate.Date = SelectedExpense.ExpenseDate;
            EditExpenseReceiptNote.Text = SelectedExpense.Note;
            var result = await EditExpenseReceiptDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Loại phiếu chi", EditExpenseReceiptCategoryComboBox.SelectedItem != null ? "valid" : "" },
                    { "Số tiền", EditExpenseReceiptAmount.Text },
                    { "Ngày", EditExpenseReceiptDate.Date != null ? "valid" : "" }
                };

                List<string> errors = _errorHandler.HandleError(fields);
                if (errors.Any())
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = string.Join("\n", errors),
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                if (!float.TryParse(EditExpenseReceiptAmount.Text, out float amount))
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Số tiền phải là số hợp lệ.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                selectedExpenseCategory = EditExpenseReceiptCategoryComboBox.SelectedItem as ExpenseCategoryModel;
                SelectedExpense.ExpenseCategoryId = selectedExpenseCategory.Id;
                SelectedExpense.Amount = amount;
                SelectedExpense.ExpenseDate = EditExpenseReceiptDate.Date.Value.DateTime;
                SelectedExpense.Note = EditExpenseReceiptNote.Text;

                await ExpenseViewModel.Update(SelectedExpense.Id.ToString(), SelectedExpense);
                await LoadDataWithProgress(ExpenseViewModel.CurrentPage);
            }
        }

        public async void showAddExpenseReceiptDialog_Click(object sender, RoutedEventArgs e)
        {
            await ExpenseCategoryViewModel.LoadData();
            AddExpenseReceiptCategoryComboBox.SelectedItem = null;
            AddExpenseReceiptAmount.Text = string.Empty;
            AddExpenseReceiptDate.Date = DateTime.Today;
            AddExpenseReceiptNote.Text = string.Empty;

            var result = await AddExpenseReceiptDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Loại phiếu chi", AddExpenseReceiptCategoryComboBox.SelectedItem != null ? "valid" : "" },
                    { "Số tiền", AddExpenseReceiptAmount.Text },
                    { "Ngày", AddExpenseReceiptDate.Date != null ? "valid" : "" }
                };

                List<string> errors = _errorHandler.HandleError(fields);
                if (errors.Any())
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = string.Join("\n", errors),
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                if (!float.TryParse(AddExpenseReceiptAmount.Text, out float amount))
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Số tiền phải là số hợp lệ.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var selectedExpenseCategory = AddExpenseReceiptCategoryComboBox.SelectedItem as ExpenseCategoryModel;
                var newExpense = new ExpenseModel
                {
                    ExpenseCategoryId = selectedExpenseCategory.Id,
                    Amount = amount,
                    ExpenseDate = AddExpenseReceiptDate.Date.Value.DateTime,
                    Note = AddExpenseReceiptNote.Text
                };

                await ExpenseViewModel.Add(newExpense);
                await LoadDataWithProgress();
            }
        }
    }
}