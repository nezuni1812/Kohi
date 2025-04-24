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
using Kohi.Errors;
using System.Threading.Tasks;

namespace Kohi.Views
{
    public sealed partial class IngredientsPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public IngredientModel? selectedIngredient { get; set; }
        public int selectedIngredientId = -1;
        private readonly IErrorHandler _errorHandler;
        public bool IsLoading { get; set; } = false;

        public IngredientsPage()
        {
            this.InitializeComponent();
            selectedIngredientId = -1;
            selectedIngredient = null;
            var emptyInputHandler = new EmptyInputErrorHandler();
            _errorHandler = emptyInputHandler;
            Loaded += IngredientsPage_Loaded;
        }

        private async void IngredientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress();
        }

        private async Task LoadDataWithProgress(int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;

                await IngredientViewModel.LoadData(page);
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading ingredients: {ex.Message}");
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is IngredientModel ingredientModel)
            {
                selectedIngredient = ingredientModel;
                selectedIngredientId = ingredientModel.Id;
                Debug.WriteLine($"Selected Ingredient ID: {selectedIngredientId}");
            }
            else
            {
                selectedIngredient = null;
                selectedIngredientId = -1;
                Debug.WriteLine("Không có nguyên vật liệu nào được chọn!");
            }
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
                await LoadDataWithProgress(selectedPage);
            }
        }

        public async void showAddIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            IngredientNameTextBox.Text = string.Empty;
            UnitTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;

            Debug.WriteLine("showAddIngredientDialog_Click triggered");
            var result = await IngredientDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Tên nguyên vật liệu", IngredientNameTextBox.Text },
                    { "Đơn vị", UnitTextBox.Text }
                };

                List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                if (errors.Any())
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = string.Join("\n", errors),
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var newIngredient = new IngredientModel
                {
                    Name = IngredientNameTextBox.Text,
                    Unit = UnitTextBox.Text,
                    Description = DescriptionTextBox.Text,
                };

                await IngredientViewModel.Add(newIngredient);
                await LoadDataWithProgress();
            }
        }

        private void AddIngredientDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            selectedIngredientId = -1;
        }

        private void AddIngredientDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public async void showEditIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIngredient == null)
            {
                ContentDialog noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có nguyên vật liệu nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            Debug.WriteLine("showEditIngredientDialog_Click triggered");
            EditIngredientNameTextBox.Text = selectedIngredient.Name;
            EditUnitTextBox.Text = selectedIngredient.Unit;
            EditDescriptionTextBox.Text = selectedIngredient.Description;

            if (await EditIngredientDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Tên nguyên vật liệu", EditIngredientNameTextBox.Text },
                    { "Đơn vị", EditUnitTextBox.Text }
                };

                List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                if (errors.Any())
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = string.Join("\n", errors),
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                IngredientModel editedIngredient = new IngredientModel
                {
                    Id = selectedIngredient.Id,
                    Name = EditIngredientNameTextBox.Text,
                    Unit = EditUnitTextBox.Text,
                    Description = EditDescriptionTextBox.Text
                };

                await IngredientViewModel.Update(selectedIngredient.Id.ToString(), editedIngredient);
                await LoadDataWithProgress(IngredientViewModel.CurrentPage);
            }
        }

        private void EditIngredientDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            selectedIngredientId = -1;
        }

        private void EditIngredientDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public async void showDeleteIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIngredientId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có nguyên vật liệu được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa nguyên vật liệu có ID là {selectedIngredientId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int res = await IngredientViewModel.Delete(selectedIngredientId.ToString());
                Debug.WriteLine($"Đã xóa nguyên vật liệu ID: {selectedIngredientId}");
                if (res == 0)
                {
                    await ShowErrorDialog("Lỗi", "Không thể xóa nguyên liệu vì có sản phẩm hoặc kho còn dùng thông tin này");
                    return;
                }
                else
                {
                    await LoadDataWithProgress(IngredientViewModel.CurrentPage);
                }
            }
        }
        private async Task ShowErrorDialog(string title, string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }
    }
}