using Microsoft.EntityFrameworkCore;
using DeliveryApi.Models;

namespace DeliveryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<ShopOwner> ShopOwners { get; set; }
    }
}
