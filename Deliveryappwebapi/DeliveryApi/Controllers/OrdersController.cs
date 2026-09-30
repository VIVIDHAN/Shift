using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryApi.Data;
using DeliveryApi.Models;

namespace DeliveryApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/orders/assigned
        // This matches what the MAUI app is calling
        [HttpPost("assigned")]
        public async Task<IActionResult> GetAssignedOrders([FromBody] OrderRequest request)
        {
            if (request.action != "getAssignedOrders" || string.IsNullOrEmpty(request.driverPhone))
            {
                return BadRequest("Invalid request.");
            }

            var orders = await _context.Orders
                .Where(o => o.DriverPhone == request.driverPhone && o.Status == "Assigned")
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/orders/complete
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteDelivery([FromForm] string action, [FromForm] string driverPhone, [FromForm] string orderId, IFormFile photo)
        {
            if (action != "completeDelivery" || string.IsNullOrEmpty(driverPhone) || string.IsNullOrEmpty(orderId))
            {
                return BadRequest("Invalid request.");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.DriverPhone == driverPhone);
            if (order == null)
            {
                return NotFound("Order not found or not assigned to this driver.");
            }

            order.Status = "Delivered";

            // Upload photo to AWS S3 if provided
            if (photo != null && photo.Length > 0)
            {
                try
                {
                    string bucketName = "shiftappimagesfolder";
                    var fileName = $"{orderId}_{DateTime.Now.Ticks}.jpg";

                    // Using AWSSDK.S3 (Ensure your AWS credentials are fixed in ~/.aws/credentials)
                    using var amazonS3Client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.APSouth1);
                    
                    using var newMemoryStream = new MemoryStream();
                    await photo.CopyToAsync(newMemoryStream);
                    
                    var uploadRequest = new Amazon.S3.Model.PutObjectRequest
                    {
                        InputStream = newMemoryStream,
                        BucketName = bucketName,
                        Key = fileName,
                        ContentType = "image/jpeg"
                    };
                    
                    await amazonS3Client.PutObjectAsync(uploadRequest);

                    // Generate the public S3 URL
                    order.DeliveryPhotoUrl = $"https://{bucketName}.s3.ap-south-1.amazonaws.com/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AWS S3 Upload Failed: {ex.Message}");
                    // We can still mark as delivered even if photo fails, or return error
                }
            }

            await _context.SaveChangesAsync();

            return Ok(); 
        }

        // POST: api/orders/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] Order newOrder)
        {
            if (newOrder == null || string.IsNullOrEmpty(newOrder.CustomerName))
            {
                return BadRequest("Invalid order data.");
            }

            // Generate a random OrderId
            newOrder.OrderId = "ORD-" + new Random().Next(1000, 9999);
            newOrder.Status = "Assigned";
            newOrder.OrderDate = DateTime.Now;

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created successfully", orderId = newOrder.OrderId });
        }

        // POST: api/orders/driver/login
        [HttpPost("driver/login")]
        public async Task<IActionResult> DriverLogin([FromBody] OrderRequest request)
        {
            if (string.IsNullOrEmpty(request.driverPhone) || string.IsNullOrEmpty(request.action)) // using action field for password to avoid changing model
            {
                return BadRequest("Invalid credentials.");
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.PhoneNumber == request.driverPhone && d.PasswordHash == request.action);
            
            if (driver != null)
            {
                return Ok(new { success = true, name = driver.FullName });
            }
            return Unauthorized(new { success = false, message = "Invalid phone number or password" });
        }

        // GET: api/orders/settings
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var shop = await _context.ShopOwners.FirstOrDefaultAsync(s => s.Id == 1);
            if (shop != null)
            {
                return Ok(new {
                    shopOwnerPhone = shop.PhoneNumber,
                    smsTemplate = shop.SmsTemplate
                });
            }
            return NotFound("Settings not found");
        }
    }
}
