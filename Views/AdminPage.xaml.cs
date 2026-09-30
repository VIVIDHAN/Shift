using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using DeliveryApp.Models;
using DeliveryApp.Services;

namespace DeliveryApp.Views
{
    public partial class AdminPage : ContentPage
    {
        public ObservableCollection<Order> AllOrders { get; set; } = new ObservableCollection<Order>();
        private ApiService _apiService = new ApiService();

        public AdminPage()
        {
            InitializeComponent();
            AllOrdersCollection.ItemsSource = AllOrders;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async void OnRefreshOrders(object sender, EventArgs e)
        {
            await LoadDataAsync();
            OrdersRefreshView.IsRefreshing = false;
        }

        private async Task LoadDataAsync()
        {
            var orders = await _apiService.GetAllOrdersAdminAsync();
            AllOrders.Clear();
            foreach(var o in orders)
            {
                AllOrders.Add(o);
            }
        }

        private async void OnCreateOrderClicked(object sender, EventArgs e)
        {
            // Simple prompt based UI to create order natively
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
                await LoadDataAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to create order.", "OK");
            }
        }
    }
}
