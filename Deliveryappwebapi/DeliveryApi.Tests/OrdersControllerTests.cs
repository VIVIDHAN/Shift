using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryApi.Controllers;
using DeliveryApi.Data;
using DeliveryApi.Models;

namespace DeliveryApi.Tests
{
    public class OrdersControllerTests
    {
        private DbContextOptions<AppDbContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task DriverLogin_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new AppDbContext(options))
            {
                context.Drivers.Add(new Driver { PhoneNumber = "12345", PasswordHash = "password", FullName = "Test Driver" });
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(options))
            {
                var controller = new OrdersController(context);
                var request = new OrderRequest { driverPhone = "12345", action = "password" };

                // Act
                var result = await controller.DriverLogin(request);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(okResult.Value);
                Assert.Contains("Test Driver", okResult.Value.ToString());
            }
        }

        [Fact]
        public async Task DriverLogin_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new AppDbContext(options))
            {
                var controller = new OrdersController(context);
                var request = new OrderRequest { driverPhone = "wrong", action = "wrong" };

                // Act
                var result = await controller.DriverLogin(request);

                // Assert
                var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Contains("Invalid phone number or password", unauthorizedResult.Value.ToString());
            }
        }
    }
}
