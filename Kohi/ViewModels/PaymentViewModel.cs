using Kohi.Models.BankingAPI;
using Newtonsoft.Json;
using PropertyChanged;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    [AddINotifyPropertyChangedInterface] 
    public class PaymentViewModel
    {
        public FullObservableCollection<Datum> Banks { get; set; } = new FullObservableCollection<Datum>();
        public Datum SelectedBank { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Amount { get; set; }
        public string QRCode { get; set; }

        public PaymentViewModel()
        {
            LoadBanks();
        }

        public async void LoadBanks()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var json = client.DownloadString("https://api.vietqr.io/v2/banks");
                    var bankData = JsonConvert.DeserializeObject<BankModel>(json);

                    Banks.Clear();
                    foreach (var bank in bankData.data)
                    {
                        Banks.Add(bank);
                    }

                    if (Banks.Count > 0)
                        SelectedBank = Banks[0]; // Chọn ngân hàng mặc định
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải ngân hàng: {ex.Message}");
            }
        }

        public async Task GenerateQRCode()
        {
            try
            {
                var apiRequest = new ApiBankingRequestModel
                {
                    acqId = Convert.ToInt32(SelectedBank.bin),
                    accountNo = long.Parse(AccountNumber),
                    accountName = AccountName,
                    amount = Convert.ToInt32(Amount),
                    format = "text",
                    template = "compact"
                };

                var jsonRequest = JsonConvert.SerializeObject(apiRequest);
                var client = new RestClient("https://api.vietqr.io/v2/generate");
                var request = new RestRequest
                {
                    Method = Method.Post
                };

                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

                var response = await client.ExecuteAsync(request);
                var content = response.Content;
                var dataResult = JsonConvert.DeserializeObject<ApiBankingResponseModel>(content);

                QRCode = dataResult.data.qrDataURL; // View sẽ tự cập nhật
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo QR: {ex.Message}");
            }
        }
    }
}
