using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Kohi.Models;
using Kohi.ViewModels;
using System.Diagnostics;
using WinUI.TableView;
using Kohi.Errors;

namespace Kohi.Views
{
    public sealed partial class InventoryOutboundPage : Page
    {
        public OutboundModel? SelectedOutbound { get; set; }
        public int SelectedOutboundId = -1;
        public OutboundViewModel OutboundViewModel { get; set; } = new OutboundViewModel();
        private readonly IErrorHandler _errorHandler;
        public bool IsLoading { get; set; } = false;

        public InventoryOutboundPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            var emptyInputHandler = new EmptyInputErrorHandler();
            var positiveNumberHandler = new PositiveNumberValidationErrorHandler(new List<string>
            {
                "Số lượng xuất"
            });
            emptyInputHandler.SetNext(positiveNumberHandler);
            _errorHandler = emptyInputHandler;
            Loaded += OutboundsPage_Loaded;
        }

        private async void OutboundsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress();
        }

        private async Task LoadDataWithProgress(int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                await OutboundViewModel.LoadData(page);
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading outbound data: {ex.Message}");
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
            if (sender is TableView tableView && tableView.SelectedItem is OutboundModel selectedOutbound)
            {
                SelectedOutbound = selectedOutbound;
                SelectedOutboundId = selectedOutbound.Id;
                OutboundBatchCodeTextBox.IsEnabled = true;
                OutboundBatchCodeTextBox.Text = selectedOutbound.Id.ToString();
                OutboundBatchCodeTextBox.IsEnabled = false;
                //editButton.IsEnabled = true;
                //deleteButton.IsEnabled = true;
                Debug.WriteLine($"Selected Outbound ID: {SelectedOutboundId}");
            }
            else
            {
                SelectedOutbound = null;
                SelectedOutboundId = -1;
                OutboundBatchCodeTextBox.Text = string.Empty;
                //editButton.IsEnabled = false;
                //deleteButton.IsEnabled = false;
                Debug.WriteLine("Không có lô hàng nào được chọn!");
            }
        }

        public void UpdatePageList()
        {
            if (OutboundViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, OutboundViewModel.TotalPages);
            pageList.SelectedItem = OutboundViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutboundViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != OutboundViewModel.CurrentPage)
            {
                await LoadDataWithProgress(selectedPage);
            }
        }

        public async void showEditInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOutboundId == -1 || SelectedOutbound == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có lô hàng nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            OutboundBatchCodeTextBox.Text = SelectedOutbound.Id.ToString();
            OutboundQuantityBox.Value = SelectedOutbound.Quantity;
            OutboundDatePicker.Date = SelectedOutbound.OutboundDate;
            OutboundReasonTextBox.Text = SelectedOutbound.Purpose ?? string.Empty;
            OutboundNotesTextBox.Text = SelectedOutbound.Notes ?? string.Empty;

            OutboundDialog.Title = "Chỉnh sửa lô hàng xuất kho";
            Debug.WriteLine("showEditInfoDialog_Click triggered");
            var result = await OutboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Số lượng xuất", OutboundQuantityBox.Text ?? "" },
                    { "Ngày xuất kho", OutboundDatePicker.Date != null ? "valid" : "" }
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

                var updatedOutbound = new OutboundModel
                {
                    Id = SelectedOutboundId,
                    InventoryId = SelectedOutbound.InventoryId,
                    Quantity = Convert.ToInt32(OutboundQuantityBox.Value),
                    OutboundDate = OutboundDatePicker.Date?.DateTime ?? DateTime.Now,
                    Purpose = OutboundReasonTextBox.Text,
                    Notes = OutboundNotesTextBox.Text
                };

                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    await OutboundViewModel.Update(SelectedOutboundId.ToString(), updatedOutbound);
                    await LoadDataWithProgress(OutboundViewModel.CurrentPage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating outbound: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể cập nhật lô hàng: {ex.Message}",
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
            OutboundDialog.Title = "Xuất kho hàng hóa";
        }

        public async void showDeleteInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOutboundId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có lô hàng nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa phiếu xuất lô hàng có ID {SelectedOutboundId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    await OutboundViewModel.Delete(SelectedOutboundId.ToString());
                    Debug.WriteLine($"Đã xóa lô hàng với ID: {SelectedOutboundId}");
                    await LoadDataWithProgress(OutboundViewModel.CurrentPage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting outbound: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể xóa lô hàng: {ex.Message}",
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
            else
            {
                Debug.WriteLine("Hủy xóa lô hàng");
            }
        }

        private void OutboundDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Không hủy mặc định để cho phép xử lý trong showEditInfoDialog_Click
        }

        private void OutboundDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedOutboundId = -1;
            OutboundBatchCodeTextBox.Text = string.Empty;
            OutboundQuantityBox.Value = 0;
            OutboundDatePicker.Date = null;
            OutboundReasonTextBox.Text = string.Empty;
            OutboundNotesTextBox.Text = string.Empty;
        }
    }
}