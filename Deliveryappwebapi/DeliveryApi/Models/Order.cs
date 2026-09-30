using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models
{
    public class Order
    {
        [Key]
        public string? OrderId { get; set; } = string.Empty;
        public string? CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; } = string.Empty;
        public string? ShopOwnerPhone { get; set; } = string.Empty;
        public string? DriverPhone { get; set; } = string.Empty;
        public string? Items { get; set; } = string.Empty;
        public string? Location { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; } = "Assigned";
        public string? DeliveryPhotoUrl { get; set; } = string.Empty;
        public string? BillPhotoUrl { get; set; } = string.Empty;
    }

    public class OrderRequest
    {
        public string action { get; set; } = string.Empty;
        public string driverPhone { get; set; } = string.Empty;
        public string orderId { get; set; } = string.Empty;
    }
}
