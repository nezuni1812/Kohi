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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Kohi.Utils;
using System.Threading.Tasks;
using Kohi.Errors;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InventoriesPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public InboundViewModel InboundViewModel { get; set; } = new InboundViewModel();
        public InventoryViewModel InventoryViewModel { get; set; } = new InventoryViewModel();
        public OutboundViewModel OutboundViewModel { get; set; } = new OutboundViewModel();
        public InventoryCheckViewModel CheckInventoryViewModel { get; set; } = new InventoryCheckViewModel();

        public InventoryModel? SelectedInventory { get; set; }
        
        public int SelectedInventoryId = -1;
        private Dictionary<string, int> _ingredientsDict;
        private Dictionary<string, int> _suppliersDict;
        private Dictionary<int, int> _inventoryDict; // Thêm để kiểm tra InventoryId
        private readonly IErrorHandler _errorHandler;
        public InventoriesPage()
        {
            this.InitializeComponent();
            Loaded += InventoriesPage_Loaded;
            this.DataContext = this;
            //GridContent.DataContext = IncomeViewModel;
            var emptyInputHandler = new EmptyInputErrorHandler();
            // Chỉ định các trường cần kiểm tra số dương
            var positiveNumberHandler = new PositiveNumberValidationErrorHandler(new List<string>
            {
                "Số lượng thực tế",
                "Số lượng xuất",
                "Số lượng nhập",
                "Tổng giá trị",
                "Số lượng", // Cho Excel
                "Tổng giá trị" // Cho Excel
            });
            emptyInputHandler.SetNext(positiveNumberHandler);
            _errorHandler = emptyInputHandler;
        }
        public async void InventoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupDataAsync();
            await InventoryViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }
        // Hàm tải danh sách nguyên liệu và nhà cung cấp
        private async Task LoadLookupDataAsync()
        {
            try
            {
                var ingredients = await IngredientViewModel.GetAll();
                var suppliers = await SupplierViewModel.GetAll();
                var inventories = await InventoryViewModel.GetAll(); // Lấy danh sách Inventory
                _ingredientsDict = ingredients?.ToDictionary(i => i.Name, i => i.Id, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>();
                _suppliersDict = suppliers?.ToDictionary(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>();
                _inventoryDict = inventories?.ToDictionary(i => i.Id, i => i.Id) ?? new Dictionary<int, int>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load lookup data: {ex.Message}");
            }
        }
        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is InventoryModel selectedInventory)
            {
                SelectedInventoryId = selectedInventory.Id;
                CheckBatchCodeTextBox.IsEnabled = true;
                CheckBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                CheckBatchCodeTextBox.IsEnabled = false;

                OutboundBatchCodeTextBox.IsEnabled = true;
                OutboundBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                OutboundBatchCodeTextBox.IsEnabled = false;
                Debug.WriteLine($"Selected Inbound ID: {SelectedInventoryId}");
            }
            else
            {
                SelectedInventory = null;
                SelectedInventoryId = -1;
                Debug.WriteLine("Không có lô hàng nào được chọn!");
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {
            //Frame rootFrame = new Frame();
            //this.Content = rootFrame;

            //rootFrame.Navigate(typeof(AddNewInventoryPage), null);
        }

        public void UpdatePageList()
        {
            if (InventoryViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, InventoryViewModel.TotalPages);
            pageList.SelectedItem = InventoryViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != InventoryViewModel.CurrentPage)
            {
                await InventoryViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }


        public async void showCheckInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
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

            Debug.WriteLine("showCheckInventoryDialog_Click triggered");
            var result = await CheckDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Mã lô hàng", CheckBatchCodeTextBox.Text },
                    { "Số lượng thực tế", InventoryQuantityBox.Text },
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

                // Thêm kiểm tra số hợp lệ trước khi lưu
                if (!int.TryParse(InventoryQuantityBox.Text, out int actualQuantity))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Số lượng thực tế phải là số hợp lệ.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                CheckInventoryModel checkInventory = new CheckInventoryModel
                {
                    InventoryId = Convert.ToInt32(CheckBatchCodeTextBox.Text),
                    ActualQuantity = Convert.ToInt32(InventoryQuantityBox.Text),
                    CheckDate = InventoryDatePicker.Date?.DateTime ?? DateTime.Now,
                    Notes = ReasonTextBox.Text, 
                };

                await CheckInventoryViewModel.Add(checkInventory);
                await InventoryViewModel.LoadData(InventoryViewModel.CurrentPage);
                UpdatePageList();
            }
        }

        public async void showOutboundInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
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

            Debug.WriteLine("showOutboundInventoryDialog_Click triggered");
            var result = await OutboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Mã lô hàng", OutboundBatchCodeTextBox.Text },
                    { "Số lượng xuất", OutboundQuantityBox.Text },
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

                // Thêm kiểm tra số hợp lệ trước khi lưu
                if (!int.TryParse(OutboundQuantityBox.Text, out int quantity))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Số lượng xuất phải là số hợp lệ.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                var inventory = InventoryViewModel.Inventories.FirstOrDefault(i => i.Id == SelectedInventoryId);
                if (inventory == null)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không tìm thấy lô hàng với mã {SelectedInventoryId}.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                if (quantity > inventory.Quantity)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = $"Số lượng xuất ({quantity}) vượt quá số lượng hiện tại ({inventory.Quantity}).",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                // OutboundBatchCodeTextBox
                OutboundModel outbound = new OutboundModel
                {
                    InventoryId = Convert.ToInt32(OutboundBatchCodeTextBox.Text),
                    Quantity = Convert.ToInt32(OutboundQuantityBox.Text),
                    OutboundDate = OutboundDatePicker.Date?.DateTime ?? DateTime.Now,
                    Purpose = OutboundReasonTextBox.Text,
                    Notes = OutboundNotesTextBox.Text,
                };
                await OutboundViewModel.Add(outbound); // Sửa từ Add() thành Add(inbound)
                await InventoryViewModel.LoadData(InventoryViewModel.CurrentPage);
                UpdatePageList();
            }
        }

        public async void showAddInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddInventoryDialog_Click triggered");
            var result = await InboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedIngredient = IngredientComboBox.SelectedItem as IngredientModel;
                var selectedSupplier = InboudSupplierComboBox.SelectedItem as SupplierModel;
                var fields = new Dictionary<string, string>
                {
                    { "Nguyên vật liệu", selectedIngredient != null ? selectedIngredient.Name : "" },
                    { "Nhà cung cấp", selectedSupplier != null ? selectedSupplier.Name : "" },
                    { "Số lượng nhập", InboundQuantityNumberBox.Value.ToString() },
                    { "Tổng giá trị", InboundTotalValueNumberBox.Value.ToString() },
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
                if (selectedIngredient == null || selectedSupplier == null)
                {
                    // Thông báo lỗi nếu không chọn Ingredient hoặc Supplier
                    // Có thể hiển thị thông báo qua UI (TextBlock hoặc ContentDialog khác)
                    Debug.WriteLine("Vui lòng chọn nguyên vật liệu và nhà cung cấp.");
                    return;
                }

                InboundModel inbound = new InboundModel
                {
                    IngredientId = selectedIngredient.Id, // Lấy ID từ Ingredient đã chọn
                    SupplierId = selectedSupplier.Id,     // Lấy ID từ Supplier đã chọn
                    Quantity = Convert.ToInt32(InboundQuantityNumberBox.Value), // Sử dụng Value thay vì Text
                    TotalCost = Convert.ToInt32(InboundTotalValueNumberBox.Value), // Sử dụng Value thay vì Text
                    InboundDate = InboundDateCalendarDatePicker.Date?.DateTime ?? DateTime.Now, // Default nếu không chọn
                    ExpiryDate = InboundExpiryDateCalendarDatePicker.Date?.DateTime ?? DateTime.Now // Default nếu không chọn
                };

                // Gọi phương thức Add từ InboundViewModel (sửa lại để truyền inbound)
                await InboundViewModel.Add(inbound); // Sửa từ Add() thành Add(inbound)

                InventoryModel inventory = new InventoryModel
                {
                    InboundId = inbound.Id,
                    Quantity = inbound.Quantity,
                    InboundDate = inbound.InboundDate,
                    ExpiryDate = inbound.ExpiryDate,
                };
                await InventoryViewModel.Add(inventory);
                Debug.WriteLine("Thêm thành công InventoryModel và Inbound");
            }
            else
            {
                Debug.WriteLine("Thêm ko thành công InventoryModel và Inbound");

            }
        }
        // Hàm xử lý sự kiện khi nhấn nút Import
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Bật ProgressRing để hiển thị trạng thái đang xử lý
            ImportProgressRing.IsActive = true;
            // Tắt nút Import để tránh nhấn liên tục
            importButton.IsEnabled = false;

            try
            {
                // Tạo FileOpenPicker để chọn file Excel
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".xlsx");

                // Khởi tạo FilePicker với cửa sổ chính
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(openPicker, hwnd);

                // Mở FilePicker và chờ người dùng chọn file
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file == null)
                {
                    // Người dùng hủy chọn file, thoát hàm
                    return;
                }

                // Đọc dữ liệu từ file Excel bằng ExcelDataReaderUtil
                List<RawInboundData> rawDataList = ExcelDataReaderUtil.ReadInboundDataFromExcel(file.Path);
                Debug.WriteLine(rawDataList[0].IngredientName);
                Debug.WriteLine(rawDataList[1].IngredientName);
                Debug.WriteLine(rawDataList[2].IngredientName);

                Debug.WriteLine(rawDataList[0].SupplierName);
                Debug.WriteLine(rawDataList[1].SupplierName);
                Debug.WriteLine(rawDataList[2].SupplierName);

                Debug.WriteLine(rawDataList[0].QuantityString);
                Debug.WriteLine(rawDataList[1].QuantityString);
                Debug.WriteLine(rawDataList[2].QuantityString);

                Debug.WriteLine(rawDataList[0].TotalCostString);
                Debug.WriteLine(rawDataList[1].TotalCostString);
                Debug.WriteLine(rawDataList[2].TotalCostString);

                Debug.WriteLine(rawDataList[0].InboundDateString);
                Debug.WriteLine(rawDataList[1].InboundDateString);
                Debug.WriteLine(rawDataList[2].InboundDateString);

                Debug.WriteLine(rawDataList[0].ExpiryDateString);
                Debug.WriteLine(rawDataList[1].ExpiryDateString);
                Debug.WriteLine(rawDataList[2].ExpiryDateString);
                foreach (var rawData in rawDataList)
                {
                    // Kiểm tra các trường bắt buộc và ánh xạ IngredientName, SupplierName sang ID
                    if (rawData.ParsedQuantity.HasValue && rawData.ParsedTotalCost.HasValue && rawData.ParsedInboundDate.HasValue &&
                        _ingredientsDict.TryGetValue(rawData.IngredientName, out int ingredientId) &&
                        _suppliersDict.TryGetValue(rawData.SupplierName, out int supplierId))
                    {
                        // Tạo InboundModel từ dữ liệu Excel
                        InboundModel inbound = new InboundModel
                        {
                            IngredientId = ingredientId,
                            SupplierId = supplierId,
                            Quantity = rawData.ParsedQuantity.Value,
                            TotalCost = rawData.ParsedTotalCost.Value,
                            InboundDate = rawData.ParsedInboundDate.Value,
                            ExpiryDate = rawData.ParsedExpiryDate ?? DateTime.Now,
                            Notes = rawData.Notes,
                        };
                        Debug.WriteLine(inbound.IngredientId);
                        Debug.WriteLine(inbound.SupplierId);
                        Debug.WriteLine(inbound.Quantity);
                        Debug.WriteLine(inbound.TotalCost);
                        Debug.WriteLine(inbound.InboundDate);
                        Debug.WriteLine(inbound.ExpiryDate);

                        // Thêm InboundModel vào ViewModel
                        await InboundViewModel.Add(inbound);
                        Debug.WriteLine(inbound.Id);
                        if (inbound.Id > 0)
                        {
                            // Tạo InventoryModel tương ứng
                            InventoryModel inventory = new InventoryModel
                            {
                                InboundId = inbound.Id,
                                Quantity = inbound.Quantity,
                                InboundDate = inbound.InboundDate,
                                ExpiryDate = inbound.ExpiryDate
                            };
                            Debug.WriteLine(inventory.InboundId);
                            Debug.WriteLine(inventory.Quantity);
                            Debug.WriteLine(inventory.InboundDate);
                            Debug.WriteLine(inventory.ExpiryDate);
                            // Thêm InventoryModel vào ViewModel
                            await InventoryViewModel.Add(inventory);
                            Debug.WriteLine(inventory.Id);
                        }
                    }
                }

                // Làm mới TableView với trang hiện tại
                await InventoryViewModel.LoadData(InventoryViewModel.CurrentPage);
                UpdatePageList();
                // Hiển thị thông báo thành công
                await ShowMessageDialog("Success", "Imported data from selected file.");
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có
                await ShowMessageDialog("Error", $"Failed to import: {ex.Message}");
            }
            finally
            {
                // Tắt ProgressRing và bật lại nút Import
                ImportProgressRing.IsActive = false;
                importButton.IsEnabled = true;
            }
        }

        // Hàm xử lý nhập xuất kho từ Excel
        private async void ImportOutboundButton_Click(object sender, RoutedEventArgs e)
        {
            ImportProgressRing.IsActive = true;
            importOutboundButton.IsEnabled = false;

            try
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".xlsx");

                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(openPicker, hwnd);

                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file == null)
                {
                    return;
                }

                List<RawOutboundData> rawDataList = ExcelDataReaderUtil.ReadOutboundDataFromExcel(file.Path);
                Debug.WriteLine($"Total outbound rows read from Excel: {rawDataList.Count}");
                if (rawDataList.Count == 0)
                {
                    await ShowMessageDialog("Warning", "No valid outbound data found in the Excel file.");
                    return;
                }
                // Thêm: Thu thập lỗi từ tất cả dòng
                List<string> errors = new List<string>();
                foreach (var rawData in rawDataList)
                {
                    // Kiểm tra lỗi với _errorHandler
                    var fields = new Dictionary<string, string>
                    {
                        { "Mã lô hàng", rawData.InventoryIdString },
                        { "Số lượng xuất", rawData.QuantityString },
                    };

                    errors.AddRange(_errorHandler?.HandleError(fields) ?? new List<string>());

                    // Kiểm tra tính hợp lệ của dữ liệu
                    if (!rawData.ParsedInventoryId.HasValue)
                    {
                        errors.Add($"Dòng {rawData.RowNumber}: Mã lô hàng không hợp lệ ({rawData.InventoryIdString}).");
                    }
                    if (!rawData.ParsedQuantity.HasValue)
                    {
                        errors.Add($"Dòng {rawData.RowNumber}: Số lượng xuất không hợp lệ ({rawData.QuantityString}).");
                    }
                    if (!rawData.ParsedOutboundDate.HasValue)
                    {
                        errors.Add($"Dòng {rawData.RowNumber}: Ngày xuất kho không hợp lệ ({rawData.OutboundDateString}).");
                    }
                    if (rawData.ParsedInventoryId.HasValue && !_inventoryDict.ContainsKey(rawData.ParsedInventoryId.Value))
                    {
                        errors.Add($"Dòng {rawData.RowNumber}: Không tìm thấy lô hàng với mã {rawData.ParsedInventoryId}.");
                    }

                    // Thêm: Kiểm tra số lượng xuất không vượt quá số lượng hiện tại
                    if (rawData.ParsedInventoryId.HasValue && rawData.ParsedQuantity.HasValue)
                    {
                        var inventory = InventoryViewModel.Inventories.FirstOrDefault(i => i.Id == rawData.ParsedInventoryId.Value);
                        if (inventory == null)
                        {
                            errors.Add($"Dòng {rawData.RowNumber}: Không tìm thấy lô hàng với mã {rawData.ParsedInventoryId}.");
                        }
                        else if (rawData.ParsedQuantity.Value > inventory.Quantity)
                        {
                            errors.Add($"Dòng {rawData.RowNumber}: Số lượng xuất ({rawData.ParsedQuantity.Value}) vượt quá số lượng hiện tại ({inventory.Quantity}).");
                        }
                    }
                }

                // Thêm: Nếu có lỗi, hiển thị ContentDialog và dừng
                if (errors.Any())
                {
                    await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                    return;
                }
                int successCount = 0;
                foreach (var rawData in rawDataList)
                {
                    if (!rawData.ParsedInventoryId.HasValue)
                        Debug.WriteLine($"Row {rawData.RowNumber}: Invalid InventoryId = {rawData.InventoryIdString}");
                    if (!rawData.ParsedQuantity.HasValue)
                        Debug.WriteLine($"Row {rawData.RowNumber}: Invalid Quantity = {rawData.QuantityString}");
                    if (!rawData.ParsedOutboundDate.HasValue)
                        Debug.WriteLine($"Row {rawData.RowNumber}: Invalid OutboundDate = {rawData.OutboundDateString}");
                    if (!_inventoryDict.ContainsKey(rawData.ParsedInventoryId ?? 0))
                        Debug.WriteLine($"Row {rawData.RowNumber}: InventoryId '{rawData.ParsedInventoryId}' not found in _inventoryDict");

                    if (rawData.ParsedInventoryId.HasValue && rawData.ParsedQuantity.HasValue && rawData.ParsedOutboundDate.HasValue &&
                        _inventoryDict.ContainsKey(rawData.ParsedInventoryId.Value))
                    {
                        OutboundModel outbound = new OutboundModel
                        {
                            InventoryId = rawData.ParsedInventoryId.Value,
                            Quantity = rawData.ParsedQuantity.Value,
                            OutboundDate = rawData.ParsedOutboundDate.Value,
                            Purpose = rawData.Purpose,
                            Notes = rawData.Notes
                        };
                        Debug.WriteLine($"Outbound: InventoryId = {outbound.InventoryId}, Quantity = {outbound.Quantity}, " +
                                        $"OutboundDate = {outbound.OutboundDate}, Purpose = {outbound.Purpose}, Notes = {outbound.Notes}");

                        await OutboundViewModel.Add(outbound);
                        Debug.WriteLine($"Outbound Id after Add: {outbound.Id}");
                        successCount++;
                    }
                }

                await ShowMessageDialog("Success", $"Imported {successCount} of {rawDataList.Count} outbound rows from the Excel file.");
                await InventoryViewModel.LoadData(InventoryViewModel.CurrentPage);
                UpdatePageList();
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Failed to import outbound data: {ex.Message}");
            }
            finally
            {
                ImportProgressRing.IsActive = false;
                importOutboundButton.IsEnabled = true;
            }
        }

        // Hàm hiển thị thông báo (ContentDialog) cho thành công/lỗi
        private async Task ShowMessageDialog(string title, string content, string closeButtonText = "OK")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap },
                CloseButtonText = closeButtonText,
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        private void CheckDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void CheckDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void OutboundDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void OutboundDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void InboundDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void InboundDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
        private void IngredientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IngredientComboBox.SelectedItem is IngredientModel selectedIngredient)
            {
                UnitTextBlock.IsEnabled = true;
                UnitTextBlock.Text = selectedIngredient.Unit ?? "Chưa có đơn vị";
                UnitTextBlock.IsEnabled = false;
            }
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {

        }

    }
}
