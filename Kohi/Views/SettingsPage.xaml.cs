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
using System.Diagnostics;
using Kohi.Models.BankingAPI;
using Newtonsoft.Json;
using RestSharp;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Net;

namespace Kohi.Views
{
    public sealed partial class SettingsPage : Page
    {
        public PaymentViewModel PaymentViewModel = new PaymentViewModel();

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadData();
            this.DataContext = PaymentViewModel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveUserPaymentSettings();

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Thành công",
                    Content = "Đã lưu thông tin thành công!",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Lỗi khi lưu thông tin: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                if (!settings.Values.ContainsKey("UserPayment"))
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Chưa có thông tin được lưu. Vui lòng lưu thông tin trước khi kiểm tra.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                string json = settings.Values["UserPayment"]?.ToString();
                var saved = JsonConvert.DeserializeObject<UserPaymentSettings>(json);

                var apiRequest = new ApiBankingRequestModel
                {
                    acqId = Convert.ToInt32(saved.BankBin),
                    accountNo = long.Parse(saved.AccountNo),
                    accountName = saved.AccountName,
                    amount = Convert.ToInt32(txtSoTien.Text),
                    format = "text",
                    template = saved.Template
                };

                var jsonRequest = JsonConvert.SerializeObject(apiRequest);
                var client = new RestClient("https://api.vietqr.io/v2/generate");
                var request = new RestRequest();

                request.Method = Method.Post;
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

                var response = await client.ExecuteAsync(request);
                var content = response.Content;
                var dataResult = JsonConvert.DeserializeObject<ApiBankingResponseModel>(content);

                qrPicture.Source = await Base64ToImageAsync(dataResult.data.qrDataURL.Replace("data:image/png;base64,", ""));
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Lỗi khi tạo mã QR: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void LoadData()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var htmlData = client.DownloadData("https://api.vietqr.io/v2/banks");
                    var bankRawJson = Encoding.UTF8.GetString(htmlData);
                    var listBankData = JsonConvert.DeserializeObject<BankModel>(bankRawJson);

                    cb_nganhang.ItemsSource = listBankData.data;

                    if (listBankData.data.Any())
                    {
                        cb_nganhang.SelectedIndex = 0;
                    }

                    cb_template.SelectedIndex = 0;
                    RestoreUserPaymentSettings();
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Lỗi",
                    Content = $"Lỗi tải danh sách ngân hàng: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        public async Task<BitmapImage> Base64ToImageAsync(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                {
                    throw new Exception("Chuỗi Base64 không hợp lệ");
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);
                BitmapImage image = new BitmapImage();

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageBytes);
                        await writer.StoreAsync();
                    }

                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                }

                return image;
            }
            catch (FormatException ex)
            {
                throw new Exception("Lỗi: Dữ liệu Base64 không hợp lệ", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi chuyển đổi Base64 thành hình ảnh: {ex.Message}", ex);
            }
        }

        private void SaveUserPaymentSettings()
        {
            var info = new UserPaymentSettings
            {
                AccountNo = txtSTK.Text,
                AccountName = txtTenTaiKhoan.Text,
                BankBin = (cb_nganhang.SelectedItem as Datum)?.bin,
                Template = ((ComboBoxItem)cb_template.SelectedItem)?.Content?.ToString(),
                Address = txtDiaChi.Text,
                Street = txtStreet.Text,
                Ward = txtWard.Text,
                District = txtDistrict.Text,
                City = txtCity.Text
            };

            string json = JsonConvert.SerializeObject(info);
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["UserPayment"] = json;
        }

        private void RestoreUserPaymentSettings()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("UserPayment"))
            {
                try
                {
                    string json = settings.Values["UserPayment"]?.ToString();
                    var saved = JsonConvert.DeserializeObject<UserPaymentSettings>(json);

                    txtSTK.Text = saved.AccountNo ?? "";
                    txtTenTaiKhoan.Text = saved.AccountName ?? "";
                    txtStreet.Text = saved.Street ?? "";
                    txtWard.Text = saved.Ward ?? "";
                    txtDistrict.Text = saved.District ?? "";
                    txtCity.Text = saved.City ?? "";
                    txtDiaChi.Text = saved.Address ?? "";

                    var foundBank = cb_nganhang.Items.Cast<Datum>().FirstOrDefault(x => x.bin == saved.BankBin);
                    if (foundBank != null)
                    {
                        cb_nganhang.SelectedItem = foundBank;
                    }

                    foreach (ComboBoxItem item in cb_template.Items)
                    {
                        if (item.Content.ToString() == saved.Template)
                        {
                            cb_template.SelectedItem = item;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Lỗi khôi phục dữ liệu: " + ex.Message);
                }
            }
        }

        private void AddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtDiaChi.Text = GetFullAddress();
        }

        private string GetFullAddress()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(txtStreet.Text))
                parts.Add(txtStreet.Text.Trim());
            if (!string.IsNullOrWhiteSpace(txtWard.Text))
                parts.Add(txtWard.Text.Trim());
            if (!string.IsNullOrWhiteSpace(txtDistrict.Text))
                parts.Add(txtDistrict.Text.Trim());
            if (!string.IsNullOrWhiteSpace(txtCity.Text))
                parts.Add(txtCity.Text.Trim());

            return string.Join(", ", parts);
        }
    }
}