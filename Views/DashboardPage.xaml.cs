using System.Collections.ObjectModel;
using System.Windows.Input;
using DeliveryApp.Models;

namespace DeliveryApp.Views;

public partial class DashboardPage : ContentPage
{
    private List<Order> _allOrdersCache;
    public ObservableCollection<Order> Orders { get; set; }
    public ICommand ReachedLocationCommand { get; set; }
    public ICommand CallCustomerCommand { get; set; }

    public DashboardPage()
    {
        InitializeComponent();
        
        _allOrdersCache = new List<Order>();
        Orders = new ObservableCollection<Order>();
        ReachedLocationCommand = new Command<Order>(async (order) => await OnReachedLocation(order));
        CallCustomerCommand = new Command<Order>(OnCallCustomer);
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
            
            _allOrdersCache.Clear();
            if (fetchedOrders != null && fetchedOrders.Count > 0)
            {
                _allOrdersCache.AddRange(fetchedOrders);
            }
            
            FilterOrders("All");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load orders: {ex.Message}");
            _allOrdersCache.Clear();
            Orders.Clear();
        }
    }

    private void FilterOrders(string filter)
    {
        Orders.Clear();
        foreach(var order in _allOrdersCache)
        {
            if (filter == "All")
                Orders.Add(order);
            else if (filter == "Pending" && order.Status != "Delivered")
                Orders.Add(order);
            else if (filter == "Completed" && order.Status == "Delivered")
                Orders.Add(order);
        }
        
        // Update badges
        TabAllBadgeLabel.Text = _allOrdersCache.Count.ToString();
        TabPendingBadgeLabel.Text = _allOrdersCache.Count(o => o.Status != "Delivered").ToString();
        TabCompletedBadgeLabel.Text = _allOrdersCache.Count(o => o.Status == "Delivered").ToString();
        
        // Update Header Progress
        int total = _allOrdersCache.Count;
        int completed = _allOrdersCache.Count(o => o.Status == "Delivered");
        
        HeaderProgressText.Text = $"{completed} of {total} completed";
        
        if (total > 0)
        {
            double percentage = (double)completed / total;
            int pct = (int)(percentage * 100);
            HeaderProgressPercentage.Text = $"{pct}%";
            
            HeaderProgressGrid.ColumnDefinitions[0].Width = new GridLength(pct, GridUnitType.Star);
            HeaderProgressGrid.ColumnDefinitions[1].Width = new GridLength(100 - pct, GridUnitType.Star);
        }
        else
        {
            HeaderProgressPercentage.Text = "0%";
            HeaderProgressGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            HeaderProgressGrid.ColumnDefinitions[1].Width = new GridLength(100, GridUnitType.Star);
        }
    }

    private void UpdateTabStyles(Border selectedBorder, Label selectedLabel, Border selectedBadgeBorder, Label selectedBadgeLabel,
                                 Border b1, Label l1, Border bb1, Label bl1,
                                 Border b2, Label l2, Border bb2, Label bl2)
    {
        // Selected
        selectedBorder.BackgroundColor = Color.FromArgb("#EFF6FF");
        selectedLabel.TextColor = Color.FromArgb("#1E4ED8");
        selectedLabel.FontAttributes = FontAttributes.Bold;
        selectedBadgeBorder.BackgroundColor = Color.FromArgb("#DBEAFE");
        selectedBadgeLabel.TextColor = Color.FromArgb("#1E4ED8");

        // Unselected 1
        b1.BackgroundColor = Colors.Transparent;
        l1.TextColor = Color.FromArgb("#6B7280");
        l1.FontAttributes = FontAttributes.None;
        bb1.BackgroundColor = Color.FromArgb("#F3F4F6");
        bl1.TextColor = Color.FromArgb("#6B7280");

        // Unselected 2
        b2.BackgroundColor = Colors.Transparent;
        l2.TextColor = Color.FromArgb("#6B7280");
        l2.FontAttributes = FontAttributes.None;
        bb2.BackgroundColor = Color.FromArgb("#F3F4F6");
        bl2.TextColor = Color.FromArgb("#6B7280");
    }

    private void OnTabAllClicked(object sender, TappedEventArgs e)
    {
        UpdateTabStyles(TabAllBorder, TabAllLabel, TabAllBadgeBorder, TabAllBadgeLabel,
                        TabPendingBorder, TabPendingLabel, TabPendingBadgeBorder, TabPendingBadgeLabel,
                        TabCompletedBorder, TabCompletedLabel, TabCompletedBadgeBorder, TabCompletedBadgeLabel);
        FilterOrders("All");
    }

    private void OnTabPendingClicked(object sender, TappedEventArgs e)
    {
        UpdateTabStyles(TabPendingBorder, TabPendingLabel, TabPendingBadgeBorder, TabPendingBadgeLabel,
                        TabAllBorder, TabAllLabel, TabAllBadgeBorder, TabAllBadgeLabel,
                        TabCompletedBorder, TabCompletedLabel, TabCompletedBadgeBorder, TabCompletedBadgeLabel);
        FilterOrders("Pending");
    }

    private void OnTabCompletedClicked(object sender, TappedEventArgs e)
    {
        UpdateTabStyles(TabCompletedBorder, TabCompletedLabel, TabCompletedBadgeBorder, TabCompletedBadgeLabel,
                        TabAllBorder, TabAllLabel, TabAllBadgeBorder, TabAllBadgeLabel,
                        TabPendingBorder, TabPendingLabel, TabPendingBadgeBorder, TabPendingBadgeLabel);
        FilterOrders("Completed");
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

    private async void OnLogoutClicked(object sender, TappedEventArgs e)
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

    private void OnCallCustomer(Order order)
    {
        if (order != null && !string.IsNullOrEmpty(order.CustomerPhone))
        {
            try
            {
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(order.CustomerPhone);
            }
            catch (Exception)
            {
                DisplayAlert("Error", "Could not open phone dialer.", "OK");
            }
        }
    }
}
