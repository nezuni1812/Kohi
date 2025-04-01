using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;
using Kohi.Models.BankingAPI;

namespace Kohi.Services
{
    public class ApiBankingService
    {
        private readonly RestClient client = new RestClient("https://api.vietqr.io/v2/");

        public async Task<ApiBankingResponseModel> GenerateQRCodeAsync(ApiBankingRequestModel request)
        {
            var jsonRequest = JsonConvert.SerializeObject(request);
            var restRequest = new RestRequest("generate", Method.Post);

            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(restRequest);
            return JsonConvert.DeserializeObject<ApiBankingResponseModel>(response.Content);
        }

        public async Task<BankModel> GetBankListAsync()
        {
            using (WebClient webClient = new WebClient())
            {
                var htmlData = await webClient.DownloadDataTaskAsync("https://api.vietqr.io/v2/banks");
                var bankRawJson = Encoding.UTF8.GetString(htmlData);
                return JsonConvert.DeserializeObject<BankModel>(bankRawJson);
            }
        }
    }
}
