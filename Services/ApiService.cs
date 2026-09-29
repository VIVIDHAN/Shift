using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeliveryApp.Models;

namespace DeliveryApp.Services
{
    public class ApiService
    {
        // 1. Local URL for testing on a physical phone on the same Wi-Fi network
        private readonly string _localUrl = "http://192.168.31.175:5287/api/";
        
        // 2. Live URL (AWS Lambda) - We will switch to this when going live
        private readonly string _liveUrl = "https://shvfrpgoe7hvwgcv57ua4edeku0kymaj.lambda-url.ap-south-1.on.aws/";

        // 3. Toggle this flag to true to test locally, or false for production
        private readonly bool _useLocalBackend = true;

        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        private string GetBaseUrl()
        {
            return _useLocalBackend ? _localUrl : _liveUrl;
        }

        public async Task<bool> DriverLoginAsync(string phone, string password)
        {
            try
            {
                // We reuse the OrderRequest struct logic on the backend for simplicity
                var payload = new { action = password, driverPhone = phone };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = _useLocalBackend ? _localUrl + "orders/driver/login" : _liveUrl;
                var response = await _httpClient.PostAsync(url, content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Order>> GetAssignedOrdersAsync(string driverPhone)
        {
            try
            {
                var payload = new { action = "getAssignedOrders", driverPhone = driverPhone };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = GetBaseUrl();
                
                // If you build a new local backend, you might want to route it to a specific endpoint
                if (_useLocalBackend)
                {
                    url += "orders/assigned"; 
                }

                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var fetchedOrders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return fetchedOrders ?? new List<Order>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Request Failed: {ex.Message}");
            }
            
            return new List<Order>();
        }

        public async Task<ShopSettings> GetSettingsAsync()
        {
            try
            {
                string url = _useLocalBackend ? _localUrl + "orders/settings" : _liveUrl;
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ShopSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get settings: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> CompleteDeliveryAsync(string orderId, string driverPhone)
        {
            try
            {
                var payload = new { action = "completeDelivery", orderId = orderId, driverPhone = driverPhone };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                string url = _useLocalBackend ? _localUrl + "orders/complete" : _liveUrl;
                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing delivery: {ex.Message}");
                return false;
            }
        }
    }
}
