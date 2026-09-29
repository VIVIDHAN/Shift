using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ShopOwner
    {
        [Key]
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SmsTemplate { get; set; } = string.Empty;
    }
}
