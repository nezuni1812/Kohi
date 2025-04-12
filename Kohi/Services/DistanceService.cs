using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kohi.Services
{
    public class DistanceService
    {
        private readonly RestClient _nominatimClient;
        private readonly RestClient _osrmClient;

        public DistanceService()
        {
            _nominatimClient = new RestClient("https://nominatim.openstreetmap.org");
            _nominatimClient.AddDefaultHeader("User-Agent", "DistanceCalculatorApp");
            _osrmClient = new RestClient("http://router.project-osrm.org");
            _osrmClient.AddDefaultHeader("User-Agent", "DistanceCalculatorApp");
        }

        public async Task<double> CalculateDistanceAsync(string address1, string address2)
        {
            if (string.IsNullOrWhiteSpace(address1) || string.IsNullOrWhiteSpace(address2))
            {
                throw new ArgumentException("Cả hai địa chỉ không được để trống.");
            }

            try
            {
                // Lấy tọa độ từ địa chỉ
                var (coord1, displayName1) = await GetCoordinatesAndDisplayNameAsync(address1);
                var (coord2, displayName2) = await GetCoordinatesAndDisplayNameAsync(address2);

                if (!coord1.HasValue || !coord2.HasValue)
                {
                    Debug.WriteLine($"Không tìm thấy tọa độ: address1={address1}, displayName1={displayName1}; address2={address2}, displayName2={displayName2}");
                    throw new Exception("Không thể tìm thấy tọa độ cho một hoặc cả hai địa chỉ.");
                }

                // Tính khoảng cách thực tế bằng OSRM
                double distance = await GetDistanceAsync(coord1.Value.Lat, coord1.Value.Lon, coord2.Value.Lat, coord2.Value.Lon);
                Debug.WriteLine($"Khoảng cách tính được: {distance:F2} km giữa {displayName1} và {displayName2}");
                return distance;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tính khoảng cách: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception($"Lỗi khi tính khoảng cách: {ex.Message}");
            }
        }

        private async Task<((double Lat, double Lon)? Coordinates, string DisplayName)> GetCoordinatesAndDisplayNameAsync(string address)
        {
            var request = new RestRequest("search", Method.Get);
            request.AddParameter("q", $"{address}, Vietnam");
            request.AddParameter("format", "json");

            var response = await _nominatimClient.ExecuteAsync(request);
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                Debug.WriteLine($"Yêu cầu Nominatim thất bại cho địa chỉ: {address}. Status: {response.StatusCode}, Content: {response.Content}");
                return (null, "Không tìm thấy");
            }

            try
            {
                var results = JsonConvert.DeserializeObject<List<LocationResult>>(response.Content);
                if (results != null && results.Any())
                {
                    if (double.TryParse(results[0].Lat, out double lat) && double.TryParse(results[0].Lon, out double lon))
                    {
                        Debug.WriteLine($"Tọa độ cho {address}: Lat={lat}, Lon={lon}, DisplayName={results[0].DisplayName}");
                        return ((lat, lon), results[0].DisplayName);
                    }
                    else
                    {
                        Debug.WriteLine($"Không thể phân tích tọa độ từ phản hồi Nominatim: {response.Content}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Không tìm thấy kết quả từ Nominatim cho địa chỉ: {address}. Content: {response.Content}");
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Lỗi phân tích JSON từ Nominatim cho địa chỉ {address}: {ex.Message}\nContent: {response.Content}");
            }

            return (null, "Không tìm thấy");
        }

        private async Task<double> GetDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            var request = new RestRequest($"route/v1/driving/{lon1},{lat1};{lon2},{lat2}", Method.Get);
            request.AddParameter("overview", "false");

            var response = await _osrmClient.ExecuteAsync(request);
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                Debug.WriteLine($"Yêu cầu OSRM thất bại. Status: {response.StatusCode}, Content: {response.Content}");
                throw new Exception("Không thể tính khoảng cách từ OSRM.");
            }

            try
            {
                var result = JsonConvert.DeserializeObject<OsrmResponse>(response.Content);
                if (result?.Routes != null && result.Routes.Any())
                {
                    double distanceInMeters = result.Routes[0].Distance;
                    Debug.WriteLine($"Khoảng cách từ OSRM: {distanceInMeters / 1000:F2} km (Lat1={lat1}, Lon1={lon1}, Lat2={lat2}, Lon2={lon2})");
                    return distanceInMeters / 1000; // Chuyển sang kilômét
                }
                else
                {
                    Debug.WriteLine($"Phản hồi OSRM không chứa tuyến đường hợp lệ: {response.Content}");
                    throw new Exception("Không tìm thấy tuyến đường hợp lệ từ OSRM.");
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Lỗi phân tích JSON từ OSRM: {ex.Message}\nContent: {response.Content}");
                throw new Exception("Không thể phân tích phản hồi từ OSRM.");
            }
        }

        // Class để deserialize JSON từ Nominatim
        private class LocationResult
        {
            [JsonProperty("lat")]
            public string Lat { get; set; }

            [JsonProperty("lon")]
            public string Lon { get; set; }

            [JsonProperty("display_name")]
            public string DisplayName { get; set; }
        }

        // Class để deserialize JSON từ OSRM
        private class OsrmResponse
        {
            [JsonProperty("routes")]
            public List<Route> Routes { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; } // Thêm để kiểm tra trạng thái phản hồi
        }

        private class Route
        {
            [JsonProperty("distance")]
            public double Distance { get; set; }
        }
    }
}