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
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Kohi.Utils;
using Kohi.Errors;

namespace Kohi.Views
{
    public sealed partial class InventoriesPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel(true);
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public InboundViewModel InboundViewModel { get; set; } = new InboundViewModel();
        public InventoryViewModel InventoryViewModel { get; set; } = new InventoryViewModel();
        public OutboundViewModel OutboundViewModel { get; set; } = new OutboundViewModel();
        public InventoryCheckViewModel CheckInventoryViewModel { get; set; } = new InventoryCheckViewModel();

        public InventoryModel? SelectedInventory { get; set; }
        public int SelectedInventoryId = -1;
        private Dictionary<string, int> _ingredientsDict;
        private Dictionary<string, int> _suppliersDict;
        private Dictionary<int, int> _inventoryDict;
        private readonly IErrorHandler _errorHandler;
        public bool IsLoading { get; set; } = false;

        public InventoriesPage()
        {
            this.InitializeComponent();
            Loaded += InventoriesPage_Loaded;
            this.DataContext = this;
            var emptyInputHandler = new EmptyInputErrorHandler();
            var dateRangeHandler = new DateRangeValidationErrorHandler("Ngày nhập kho", "Ngày hết hạn");
            var positiveNumberHandler = new PositiveNumberValidationErrorHandler(new List<string>
            {
                "Số lượng thực tế",
                "Số lượng xuất",
                "Số lượng nhập",
                "Tổng giá trị",
                "Số lượng",
                "Tổng giá trị"
            });
            emptyInputHandler.SetNext(dateRangeHandler);
            dateRangeHandler.SetNext(positiveNumberHandler);
            _errorHandler = emptyInputHandler;
        }

        private async void InventoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress();
        }

        private async Task LoadDataWithProgress(int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                await LoadLookupDataAsync();
                await InventoryViewModel.LoadData(page);
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                await ShowMessageDialog("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
            }
        }

        private async Task LoadLookupDataAsync()
        {
            try
            {
                var ingredients = await IngredientViewModel.GetAll();
                var suppliers = await SupplierViewModel.GetAll();
                var inventories = await InventoryViewModel.GetAll();
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
                SelectedInventory = selectedInventory;
                CheckBatchCodeTextBox.IsEnabled = true;
                CheckBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                CheckBatchCodeTextBox.IsEnabled = false;

                OutboundBatchCodeTextBox.IsEnabled = true;
                OutboundBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                OutboundBatchCodeTextBox.IsEnabled = false;
                Debug.WriteLine($"Selected Inbound ID: {SelectedInventoryId}");
                editButton.IsEnabled = true;
                checkButton.IsEnabled = true;
            }
            else
            {
                SelectedInventory = null;
                SelectedInventoryId = -1;
                Debug.WriteLine("Không có lô hàng nào được chọn!");
                editButton.IsEnabled = false;
                checkButton.IsEnabled = false;
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {
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
                await LoadDataWithProgress(selectedPage);
            }
        }

        public async void showCheckInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
            {
                await ShowMessageDialog("Lỗi", "Không có lô hàng nào được chọn");
                return;
            }

            Debug.WriteLine("showCheckInventoryDialog_Click triggered");
            var result = await CheckDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    var fields = new Dictionary<string, string>
                    {
                        { "Mã lô hàng", CheckBatchCodeTextBox.Text },
                        { "Số lượng thực tế", InventoryQuantityBox.Text },
                    };

                    List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (errors.Any())
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                        return;
                    }

                    if (!int.TryParse(InventoryQuantityBox.Text, out int actualQuantity))
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", "Số lượng thực tế phải là số hợp lệ.");
                        return;
                    }

                    CheckInventoryModel checkInventory = new CheckInventoryModel
                    {
                        InventoryId = Convert.ToInt32(CheckBatchCodeTextBox.Text),
                        ActualQuantity = actualQuantity,
                        CheckDate = InventoryDatePicker.Date?.DateTime ?? DateTime.Now,
                        Notes = ReasonTextBox.Text,
                    };

                    await CheckInventoryViewModel.Add(checkInventory);
                    await LoadDataWithProgress(InventoryViewModel.CurrentPage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking inventory: {ex.Message}");
                    await ShowMessageDialog("Lỗi", $"Không thể kiểm kê lô hàng: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                    ProgressRing.IsActive = false;
                }
            }
        }

        public async void showOutboundInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
            {
                await ShowMessageDialog("Lỗi", "Không có lô hàng nào được chọn");
                return;
            }

            Debug.WriteLine("showOutboundInventoryDialog_Click triggered");
            var result = await OutboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    var fields = new Dictionary<string, string>
                    {
                        { "Mã lô hàng", OutboundBatchCodeTextBox.Text },
                        { "Số lượng xuất", OutboundQuantityBox.Text },
                    };

                    List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (errors.Any())
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                        return;
                    }

                    if (!int.TryParse(OutboundQuantityBox.Text, out int quantity))
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", "Số lượng xuất phải là số hợp lệ.");
                        return;
                    }

                    var inventory = InventoryViewModel.Inventories.FirstOrDefault(i => i.Id == SelectedInventoryId);
                    if (inventory == null)
                    {
                        await ShowMessageDialog("Lỗi", $"Không tìm thấy lô hàng với mã {SelectedInventoryId}.");
                        return;
                    }

                    if (quantity > inventory.Quantity)
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", $"Số lượng xuất ({quantity}) vượt quá số lượng hiện tại ({inventory.Quantity}).");
                        return;
                    }

                    OutboundModel outbound = new OutboundModel
                    {
                        InventoryId = Convert.ToInt32(OutboundBatchCodeTextBox.Text),
                        Quantity = quantity,
                        OutboundDate = OutboundDatePicker.Date?.DateTime ?? DateTime.Now,
                        Purpose = OutboundReasonTextBox.Text,
                        Notes = OutboundNotesTextBox.Text,
                    };

                    await OutboundViewModel.Add(outbound);
                    await LoadDataWithProgress(InventoryViewModel.CurrentPage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error outbound inventory: {ex.Message}");
                    await ShowMessageDialog("Lỗi", $"Không thể xuất lô hàng: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                    ProgressRing.IsActive = false;
                }
            }
        }

        public async void showAddInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddInventoryDialog_Click triggered");
            var result = await InboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;
                    var inboundDate = InboundDateCalendarDatePicker.Date?.DateTime ?? DateTime.Now;
                    var selectedIngredient = IngredientComboBox.SelectedItem as IngredientModel;
                    var selectedSupplier = InboudSupplierComboBox.SelectedItem as SupplierModel;
                    var fields = new Dictionary<string, string>
                    {
                        { "Nguyên vật liệu", selectedIngredient != null ? selectedIngredient.Name : "" },
                        { "Nhà cung cấp", selectedSupplier != null ? selectedSupplier.Name : "" },
                        { "Số lượng nhập", InboundQuantityNumberBox.Value.ToString() },
                        { "Tổng giá trị", InboundTotalValueNumberBox.Value.ToString() },
                        { "Ngày nhập kho", inboundDate.ToString() },
                        { "Ngày hết hạn", InboundExpiryDateCalendarDatePicker.Date?.DateTime.ToString() ?? "" },
                    };

                    List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (errors.Any())
                    {
                        await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                        return;
                    }

                    if (selectedIngredient == null || selectedSupplier == null)
                    {
                        Debug.WriteLine("Vui lòng chọn nguyên vật liệu và nhà cung cấp.");
                        await ShowMessageDialog("Lỗi nhập liệu", "Vui lòng chọn nguyên vật liệu và nhà cung cấp.");
                        return;
                    }

                    InboundModel inbound = new InboundModel
                    {
                        IngredientId = selectedIngredient.Id,
                        SupplierId = selectedSupplier.Id,
                        Quantity = Convert.ToInt32(InboundQuantityNumberBox.Value),
                        TotalCost = Convert.ToInt32(InboundTotalValueNumberBox.Value),
                        InboundDate = inboundDate,
                        ExpiryDate = InboundExpiryDateCalendarDatePicker.Date?.DateTime ?? DateTime.Now
                    };

                    await InboundViewModel.Add(inbound);

                    InventoryModel inventory = new InventoryModel
                    {
                        InboundId = inbound.Id,
                        Quantity = inbound.Quantity,
                        InboundDate = inbound.InboundDate,
                        ExpiryDate = inbound.ExpiryDate,
                    };
                    await InventoryViewModel.Add(inventory);
                    await LoadDataWithProgress(InventoryViewModel.CurrentPage);
                    Debug.WriteLine("Thêm thành công InventoryModel và Inbound");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding inventory: {ex.Message}");
                    await ShowMessageDialog("Lỗi", $"Không thể nhập lô hàng mới: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                    ProgressRing.IsActive = false;
                }
            }
            else
            {
                Debug.WriteLine("Thêm ko thành công InventoryModel và Inbound");
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                importButton.IsEnabled = false;

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

                List<RawInboundData> rawDataList = ExcelDataReaderUtil.ReadInboundDataFromExcel(file.Path);
                List<string> errors = new List<string>();
                List<InboundModel> validInbounds = new List<InboundModel>();

                foreach (var rawData in rawDataList)
                {
                    // Xác định ngày nhập kho, mặc định là hôm nay nếu để trống
                    var inboundDate = rawData.ParsedInboundDate ?? DateTime.Now;
                    // Xác định ngày hết hạn, mặc định là hôm nay + 1 năm nếu để trống
                    var expiryDate = rawData.ParsedExpiryDate ?? DateTime.Now.AddYears(1);

                    // Tạo fields để kiểm tra lỗi, bao gồm ngày nhập kho và ngày hết hạn
                    var fields = new Dictionary<string, string>
                    {
                        { "Nguyên vật liệu", rawData.IngredientName ?? "" },
                        { "Nhà cung cấp", rawData.SupplierName ?? "" },
                        { "Số lượng nhập", rawData.QuantityString ?? "" },
                        { "Tổng giá trị", rawData.TotalCostString ?? "" },
                        { "Ngày nhập kho", inboundDate.ToString("MM/dd/yyyy") }, // MODIFIED: Use MM/dd/yyyy
                        { "Ngày hết hạn", expiryDate.ToString("MM/dd/yyyy") } // MODIFIED: Use MM/dd/yyyy
                    };

                    // Kiểm tra lỗi bằng _errorHandler (bao gồm DateRangeValidationErrorHandler)
                    var rowErrors = _errorHandler.HandleError(fields);
                    if (rowErrors.Any())
                    {
                        errors.AddRange(rowErrors.Select(error =>
                            $"Dòng {rawData.RowNumber}: {error} (Giá trị: {GetFieldValue(fields, error)})"));
                        continue; // Bỏ qua dòng này, nhưng tiếp tục kiểm tra các dòng khác
                    }

                    // Kiểm tra dữ liệu hợp lệ trước khi tạo InboundModel
                    if (rawData.ParsedQuantity.HasValue &&
                        rawData.ParsedTotalCost.HasValue &&
                        _ingredientsDict.TryGetValue(rawData.IngredientName, out int ingredientId) &&
                        _suppliersDict.TryGetValue(rawData.SupplierName, out int supplierId))
                    {
                        // Tạo InboundModel nhưng chưa thêm vào DB, lưu vào danh sách tạm
                        InboundModel inbound = new InboundModel
                        {
                            IngredientId = ingredientId,
                            SupplierId = supplierId,
                            Quantity = rawData.ParsedQuantity.Value,
                            TotalCost = rawData.ParsedTotalCost.Value,
                            InboundDate = inboundDate,
                            ExpiryDate = expiryDate,
                            Notes = rawData.Notes,
                        };
                        validInbounds.Add(inbound);
                    }
                    else
                    {
                        // Thêm thông báo lỗi chi tiết cho dòng nếu dữ liệu không hợp lệ
                        var errorDetails = new List<string>();
                        if (!rawData.ParsedQuantity.HasValue)
                            errorDetails.Add($"Số lượng nhập không hợp lệ ({rawData.QuantityString})");
                        if (!rawData.ParsedTotalCost.HasValue)
                            errorDetails.Add($"Tổng giá trị không hợp lệ ({rawData.TotalCostString})");
                        if (!_ingredientsDict.ContainsKey(rawData.IngredientName))
                            errorDetails.Add($"Không tìm thấy nguyên vật liệu ({rawData.IngredientName})");
                        if (!_suppliersDict.ContainsKey(rawData.SupplierName))
                            errorDetails.Add($"Không tìm thấy nhà cung cấp ({rawData.SupplierName})");
                        if (errorDetails.Any())
                            errors.Add($"Dòng {rawData.RowNumber}: {string.Join("; ", errorDetails)}.");
                    }
                }

                // Nếu có bất kỳ lỗi nào, hiển thị thông báo và không thêm dòng nào
                if (errors.Any())
                {
                    await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                    return;
                }

                // Chỉ thêm các InboundModel và InventoryModel nếu không có lỗi
                foreach (var inbound in validInbounds)
                {
                    await InboundViewModel.Add(inbound);
                    if (inbound.Id > 0)
                    {
                        InventoryModel inventory = new InventoryModel
                        {
                            InboundId = inbound.Id,
                            Quantity = inbound.Quantity,
                            InboundDate = inbound.InboundDate,
                            ExpiryDate = inbound.ExpiryDate
                        };
                        await InventoryViewModel.Add(inventory);
                    }
                }

                await LoadDataWithProgress(InventoryViewModel.CurrentPage);
                await ShowMessageDialog("Thành công", "Đã nhập dữ liệu từ tệp đã chọn.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to import: {ex.Message}");
                await ShowMessageDialog("Thất bại", $"Không nhập dữ liệu được vì: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
                importButton.IsEnabled = true;
            }
        }

        // Helper method to extract field value for error message
        private string GetFieldValue(Dictionary<string, string> fields, string errorMessage)
        {
            foreach (var field in fields)
            {
                if (errorMessage.Contains(field.Key))
                {
                    return $"{field.Key}: {field.Value}";
                }
            }
            return string.Join(", ", fields.Select(f => $"{f.Key}: {f.Value}"));
        }

        private async void ImportOutboundButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                importOutboundButton.IsEnabled = false;

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
                    await ShowMessageDialog("Thất bại", "Không tìm thấy dữ liệu xuất kho hợp lệ trong tệp Excel.");
                    return;
                }

                // NEW: Fetch all inventories using InventoryViewModel.GetAll()
                var allInventories = await InventoryViewModel.GetAll();
                if (allInventories == null)
                {
                    await ShowMessageDialog("Thất bại", "Không thể tải dữ liệu kho.");
                    return;
                }
                var inventoryDict = allInventories.ToDictionary(i => i.Id, i => i);

                List<string> errors = new List<string>();
                List<OutboundModel> validOutbounds = new List<OutboundModel>();

                foreach (var rawData in rawDataList)
                {
                    // Determine outbound date, default to today if empty
                    var outboundDate = rawData.ParsedOutboundDate ?? DateTime.Now;

                    // Create fields for error checking, including outbound date
                    var fields = new Dictionary<string, string>
            {
                { "Inventory ID", rawData.InventoryIdString ?? "" },
                { "Outbound Quantity", rawData.QuantityString ?? "" },
                { "Outbound Date", outboundDate.ToString("MM/dd/yyyy") }
            };

                    // Check errors using looking for errors using _errorHandler (including DateRangeValidationErrorHandler)
                    var rowErrors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (rowErrors.Any())
                    {
                        errors.AddRange(rowErrors.Select(error =>
                            $"Row {rawData.RowNumber}: {error} (Value: {GetFieldValue(fields, error)})"));
                        continue;
                    }

                    // Validate specific fields
                    if (!rawData.ParsedInventoryId.HasValue)
                    {
                        errors.Add($"Row {rawData.RowNumber}: Invalid inventory ID ({rawData.InventoryIdString}).");
                        continue;
                    }
                    if (!rawData.ParsedQuantity.HasValue)
                    {
                        errors.Add($"Row {rawData.RowNumber}: Invalid outbound quantity ({rawData.QuantityString}).");
                        continue;
                    }
                    if (!_inventoryDict.ContainsKey(rawData.ParsedInventoryId.Value))
                    {
                        errors.Add($"Row {rawData.RowNumber}: Inventory ID not found ({rawData.ParsedInventoryId}).");
                        continue;
                    }

                    // NEW: Use inventoryDict for validation instead of DAO or Inventories
                    if (!inventoryDict.TryGetValue(rawData.ParsedInventoryId.Value, out var inventory))
                    {
                        errors.Add($"Row {rawData.RowNumber}: Inventory ID not found ({rawData.ParsedInventoryId}).");
                        continue;
                    }

                    if (rawData.ParsedQuantity.Value > inventory.Quantity)
                    {
                        errors.Add($"Row {rawData.RowNumber}: Outbound quantity ({rawData.ParsedQuantity.Value}) exceeds available quantity ({inventory.Quantity}).");
                        continue;
                    }
                    if (outboundDate.Date < inventory.InboundDate.Date)
                    {
                        errors.Add($"Row {rawData.RowNumber}: Outbound date ({outboundDate:MM/dd/yyyy}) is earlier than inbound date ({inventory.InboundDate:MM/dd/yyyy}).");
                        continue;
                    }

                    // Create OutboundModel and store in temporary list
                    OutboundModel outbound = new OutboundModel
                    {
                        InventoryId = rawData.ParsedInventoryId.Value,
                        Quantity = rawData.ParsedQuantity.Value,
                        OutboundDate = outboundDate,
                        Purpose = rawData.Purpose,
                        Notes = rawData.Notes
                    };
                    validOutbounds.Add(outbound);
                }

                // If any errors exist, show errors and do not import any rows
                if (errors.Any())
                {
                    await ShowMessageDialog("Lỗi nhập liệu", string.Join("\n", errors));
                    return;
                }

                // Import all valid rows
                int successCount = 0;
                foreach (var outbound in validOutbounds)
                {
                    await OutboundViewModel.Add(outbound);
                    successCount++;
                }

                await LoadDataWithProgress(InventoryViewModel.CurrentPage);
                await ShowMessageDialog("Thành công", $"Đã nhập {successCount} trên {rawDataList.Count} dòng xuất kho từ tệp Excel.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to import outbound data: {ex.Message}");
                await ShowMessageDialog("Thất bại", $"Không thể nhập dữ liệu xuất kho: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
                importOutboundButton.IsEnabled = true;
            }
        }


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

        private void ShowExcelSampleInfoBar_Click(object sender, RoutedEventArgs e)
        {
            SampleExcelFileInfoBar.IsOpen = !SampleExcelFileInfoBar.IsOpen;
        }
    }
}