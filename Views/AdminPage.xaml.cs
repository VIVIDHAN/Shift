using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using DeliveryApp.Models;
using DeliveryApp.Services;

namespace DeliveryApp.Views
{
    public partial class AdminPage : TabbedPage
    {
        public ObservableCollection<Order> AllOrders { get; set; } = new ObservableCollection<Order>();
        public ObservableCollection<Driver> AllDrivers { get; set; } = new ObservableCollection<Driver>();
        
        private ApiService _apiService = new ApiService();
        private bool _settingsLoaded = false;

        public AdminPage()
        {
            InitializeComponent();
            AllOrdersCollection.ItemsSource = AllOrders;
            AllDriversCollection.ItemsSource = AllDrivers;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadOrdersAsync();
            await LoadDriversAsync();
            if(!_settingsLoaded)
            {
                await LoadSettingsAsync();
                _settingsLoaded = true;
            }
        }

        // --- ORDERS TAB ---
        private async void OnRefreshOrders(object sender, EventArgs e)
        {
            await LoadOrdersAsync();
            OrdersRefreshView.IsRefreshing = false;
        }

        private async Task LoadOrdersAsync()
        {
            var orders = await _apiService.GetAllOrdersAdminAsync();
            AllOrders.Clear();
            foreach(var o in orders) AllOrders.Add(o);
        }

        private async void OnCreateOrderClicked(object sender, EventArgs e)
        {
            string customerName = await DisplayPromptAsync("New Order", "Enter Customer Name:");
            if (string.IsNullOrWhiteSpace(customerName)) return;

            string phone = await DisplayPromptAsync("New Order", "Enter Customer Phone:");
            string location = await DisplayPromptAsync("New Order", "Enter Delivery Location:");
            string items = await DisplayPromptAsync("New Order", "Enter Items (e.g. 2x Cement):");
            string driverPhone = await DisplayPromptAsync("New Order", "Assign Driver Phone (Optional):");

            bool success = await _apiService.CreateOrderAsync(customerName, phone ?? "", location ?? "", items ?? "", driverPhone ?? "");
            if (success)
            {
                await DisplayAlert("Success", "Order created successfully!", "OK");
                await LoadOrdersAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to create order.", "OK");
            }
        }

        private async void OnDeleteOrderClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string orderId)
            {
                bool confirm = await DisplayAlert("Confirm", $"Are you sure you want to delete {orderId}?", "Yes", "No");
                if (confirm)
                {
                    bool ok = await _apiService.RunSqlAsync($"DELETE FROM Orders WHERE OrderId = '{orderId}'");
                    if (ok) await LoadOrdersAsync();
                }
            }
        }

        // --- DRIVERS TAB ---
        private async void OnRefreshDrivers(object sender, EventArgs e)
        {
            await LoadDriversAsync();
            DriversRefreshView.IsRefreshing = false;
        }

        private async Task LoadDriversAsync()
        {
            var drivers = await _apiService.GetAllDriversAdminAsync();
            AllDrivers.Clear();
            foreach(var d in drivers) AllDrivers.Add(d);
        }

        private async void OnAddDriverClicked(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("New Driver", "Enter Full Name:");
            if (string.IsNullOrWhiteSpace(name)) return;
            string phone = await DisplayPromptAsync("New Driver", "Enter Phone Number:");
            if (string.IsNullOrWhiteSpace(phone)) return;
            string password = await DisplayPromptAsync("New Driver", "Enter Password:");

            bool ok = await _apiService.RunSqlAsync($"INSERT INTO Drivers (FullName, PhoneNumber, PasswordHash) VALUES ('{name.Replace("'", "''")}', '{phone.Replace("'", "''")}', '{password?.Replace("'", "''")}')");
            if (ok)
            {
                await DisplayAlert("Success", "Driver added!", "OK");
                await LoadDriversAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to add driver.", "OK");
            }
        }

        private async void OnDeleteDriverClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string phone)
            {
                bool confirm = await DisplayAlert("Confirm", $"Delete driver {phone}?", "Yes", "No");
                if (confirm)
                {
                    bool ok = await _apiService.RunSqlAsync($"DELETE FROM Drivers WHERE PhoneNumber = '{phone}'");
                    if (ok) await LoadDriversAsync();
                }
            }
        }

        // --- SETTINGS TAB ---
        private async Task LoadSettingsAsync()
        {
            var settings = await _apiService.GetSettingsAsync();
            if (settings != null)
            {
                ShopNameEntry.Text = settings.ShopName;
                ShopPhoneEntry.Text = settings.ShopOwnerPhone;
                ShopAddressEntry.Text = settings.Address;
                SmsTemplateEditor.Text = settings.SmsTemplate;
            }
        }

        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            string sName = ShopNameEntry.Text?.Replace("'", "''") ?? "";
            string sPhone = ShopPhoneEntry.Text?.Replace("'", "''") ?? "";
            string sAddress = ShopAddressEntry.Text?.Replace("'", "''") ?? "";
            string sms = SmsTemplateEditor.Text?.Replace("'", "''") ?? "";

            bool ok = await _apiService.RunSqlAsync($"UPDATE ShopOwners SET ShopName = '{sName}', PhoneNumber = '{sPhone}', Address = '{sAddress}', SmsTemplate = '{sms}' WHERE Id = 1");
            if (ok)
            {
                await DisplayAlert("Success", "Settings saved!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save settings.", "OK");
            }
        }
    }
}
