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

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        var t = Services.LocalizationManager.Instance;
        if (sender is Button btn)
        {
            btn.Text = "...";
            btn.IsEnabled = false;
        }

        await LoadOrdersAsync();

        if (sender is Button restoreBtn)
        {
            restoreBtn.Text = t["Refresh"];
            restoreBtn.IsEnabled = true;
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var t = Services.LocalizationManager.Instance;
        bool confirm = await DisplayAlert(t["Logout"], t["LogoutConfirm"], t["Yes"], t["No"]);
        if (confirm)
        {
            Preferences.Remove("DriverPhone");
            
            var navPage = new NavigationPage(new LoginPage());
            navPage.BarBackgroundColor = Color.FromArgb("#1E3A8A");
            navPage.BarTextColor = Colors.White;
            Application.Current.MainPage = navPage;
        }
    }
}
