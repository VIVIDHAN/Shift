using System.Collections.ObjectModel;
using System.Windows.Input;
using DeliveryApp.Models;

namespace DeliveryApp.Views;

public partial class DashboardPage : ContentPage
{
    public ObservableCollection<Order> Orders { get; set; }
    public ICommand ReachedLocationCommand { get; set; }

    public DashboardPage()
    {
        InitializeComponent();
        
        Orders = new ObservableCollection<Order>();
        ReachedLocationCommand = new Command<Order>(async (order) => await OnReachedLocation(order));
        BindingContext = this;
        OrdersCollection.ItemsSource = Orders;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            // Connect to live AWS Lambda endpoint
            HttpClient client = new HttpClient();
            // Since lambda doesn't return JSON on GET by default now, we send a POST with an action
            var payload = new { action = "getAssignedOrders", driverPhone = Preferences.Get("DriverPhone", "") };
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://shvfrpgoe7hvwgcv57ua4edeku0kymaj.lambda-url.ap-south-1.on.aws/", content);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var fetchedOrders = System.Text.Json.JsonSerializer.Deserialize<List<Order>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                Orders.Clear();
                if (fetchedOrders != null)
                {
                    foreach (var order in fetchedOrders)
                    {
                        Orders.Add(order);
                    }
                }
            }
            else 
            {
                // API not returning JSON yet (e.g. database not hooked up)
                Orders.Clear();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load orders: {ex.Message}");
            Orders.Clear();
        }
    }

    private async Task OnReachedLocation(Order order)
    {
        await Navigation.PushAsync(new DeliveryPage(order));
    }
}
