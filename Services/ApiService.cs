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

        public async Task<bool> CompleteDeliveryAsync(string orderId, string driverPhone, byte[] photoData = null)
        {
            try
            {
                string url = _baseUrl + "orders/complete";
                
                var content = new MultipartFormDataContent();
                content.Add(new StringContent("completeDelivery"), "action");
                content.Add(new StringContent(orderId), "orderId");
                content.Add(new StringContent(driverPhone), "driverPhone");
                
                if (photoData != null)
                {
                    var imageContent = new ByteArrayContent(photoData);
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "photo", "delivery.jpg");
                }

                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing delivery: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateOrderAsync(string customerName, string customerPhone, string location, string items, string driverPhone)
        {
            try
            {
                var payload = new { 
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    Location = location,
                    Items = items,
                    DriverPhone = driverPhone
                };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = _baseUrl + "orders/create";
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Quick way to get all orders for admin
        public async Task<List<Order>> GetAllOrdersAdminAsync()
        {
            try
            {
                var payload = new { Sql = "SELECT * FROM Orders ORDER BY Id DESC" };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = _baseUrl + "admin/query";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Admin query returns { columns: [], rows: [ { "OrderId": "ORD-123", ... } ] }
                    var result = JsonSerializer.Deserialize<AdminQueryResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result?.Rows != null)
                    {
                        var ordersList = new List<Order>();
                        foreach(var r in result.Rows)
                        {
                            var o = new Order();
                            if(r.TryGetValue("OrderId", out var oid)) o.OrderId = oid.ToString();
                            if(r.TryGetValue("CustomerName", out var cn)) o.CustomerName = cn.ToString();
                            if(r.TryGetValue("Location", out var loc)) o.Location = loc.ToString();
                            if(r.TryGetValue("Items", out var items)) o.Items = items.ToString();
                            if(r.TryGetValue("Status", out var stat)) o.Status = stat.ToString();
                            if(r.TryGetValue("DriverPhone", out var dp)) o.DriverPhone = dp.ToString();
                            ordersList.Add(o);
                        }
                        return ordersList;
                    }
                }
            }
            catch (Exception)
            {
            }
            return new List<Order>();
        }
        
        public class AdminQueryResult
        {
            public List<Dictionary<string, object>> Rows { get; set; }
        }
    }
}
