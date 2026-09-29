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
        // Network address for local Mac backend (.NET Web API)
        private readonly string _baseUrl = "http://192.168.31.175:5287/api/";
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> DriverLoginAsync(string phone, string password)
        {
            try
            {
                var payload = new { action = password, driverPhone = phone };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = _baseUrl + "orders/driver/login";
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

                string url = _baseUrl + "orders/assigned";
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
                string url = _baseUrl + "orders/settings";
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

                string url = _baseUrl + "orders/complete";
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
