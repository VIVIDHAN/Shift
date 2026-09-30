namespace DeliveryApp.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string ShopOwnerPhone { get; set; }
        public string Items { get; set; }
        public string Location { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // "Assigned", "Delivered"
        public string DriverPhone { get; set; }
        public string BillPhotoUrl { get; set; }
        public string DeliveryPhotoUrl { get; set; }
        public bool HasBillPhoto => !string.IsNullOrEmpty(BillPhotoUrl);
        public bool IsAssigned => Status == "Assigned";
    }

    public class ShopSettings
    {
        public string ShopName { get; set; }
        public string ShopOwnerPhone { get; set; }
        public string Address { get; set; }
        public string SmsTemplate { get; set; }
    }
}
