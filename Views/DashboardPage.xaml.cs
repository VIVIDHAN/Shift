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
            var apiService = new Services.ApiService();
            var fetchedOrders = await apiService.GetAssignedOrdersAsync(Preferences.Get("DriverPhone", ""));
            
            Orders.Clear();
            if (fetchedOrders != null && fetchedOrders.Count > 0)
            {
                foreach (var order in fetchedOrders)
                {
                    Orders.Add(order);
                }
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
