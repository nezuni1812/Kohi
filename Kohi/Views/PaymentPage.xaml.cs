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

using Microsoft.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Net;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaymentPage : Page
    {
        public PaymentViewModel PaymentViewModel = new PaymentViewModel();
        public PaymentPage()
        {
            this.InitializeComponent();
            LoadData();
            this.DataContext = PaymentViewModel;
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var apiRequest = new ApiBankingRequestModel
                {
                    acqId = Convert.ToInt32(((Datum)cb_nganhang.SelectedItem).bin),
                    accountNo = long.Parse(txtSTK.Text),
                    accountName = txtTenTaiKhoan.Text,
                    amount = Convert.ToInt32(txtSoTien.Text),
                    format = "text",
                    template = ((ComboBoxItem)cb_template.SelectedItem).Content.ToString()
                };

                var jsonRequest = JsonConvert.SerializeObject(apiRequest);
                var client = new RestClient("https://api.vietqr.io/v2/generate");
                var request = new RestRequest();

                request.Method = Method.Post;
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;
                var dataResult = JsonConvert.DeserializeObject<ApiBankingResponseModel>(content);

                // Gọi phương thức Base64ToImageAsync một cách bất đồng bộ
                pictureBox1.Source = await Base64ToImageAsync(dataResult.data.qrDataURL.Replace("data:image/png;base64,", ""));
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Lỗi",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
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
                // Kiểm tra xem chuỗi Base64 có hợp lệ không
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
                // Lỗi FormatException nếu chuỗi Base64 không hợp lệ
                throw new Exception("Lỗi: Dữ liệu Base64 không hợp lệ", ex);
            }
            catch (Exception ex)
            {
                // Lỗi khác
                throw new Exception($"Lỗi chuyển đổi Base64 thành hình ảnh: {ex.Message}", ex);
            }
        }

    }
}
