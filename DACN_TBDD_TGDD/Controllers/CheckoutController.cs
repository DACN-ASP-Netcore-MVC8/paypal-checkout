using DACN_TBDD_TGDD.Areas.Admin.Repository;
using DACN_TBDD_TGDD.Helpers;
using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.Services;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal;
using PayPal.Api;
using PayPal.Api.OpenIdConnect;
using System.Net;
using System.Security.Claims;

namespace DACN_TBDD_TGDD.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<CheckoutController> _logger;
        private readonly IEmailSender _emailSender;
        


        public CheckoutController(ILogger<CheckoutController> logger, DataContext context, IEmailSender emailSender)
        {
            _logger = logger;
            _dataContext = context;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Checkout()
        {
            // Lấy email của người dùng đã đăng nhập
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _dataContext.Users
            .FirstOrDefaultAsync(u => u.Email == userEmail);
                // Kiểm tra xem người dùng đã nhập địa chỉ và số điện thoại chưa
                if (string.IsNullOrEmpty(user.Address) || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    TempData["ErrorMessage"] = "Please update your address and phone number before payment.";
                    return RedirectToAction("EditProfile", "Account"); // Giả sử có trang hồ sơ để người dùng cập nhật thông tin
                }
                // Tạo mã đơn hàng duy nhất
                var ordercode = Guid.NewGuid().ToString();

                // Tạo thông tin đơn hàng mới
                var orderItems = new OrderModel
                {
                    OrderCode = ordercode,
                    UserName = userEmail,
                    Status = 1,
                    CreatedDate = DateTime.Now,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber
                };

                // Thêm đơn hàng vào cơ sở dữ liệu
                _dataContext.Add(orderItems);

                // Lấy giỏ hàng từ session
                List<CartModel> cartsItems = HttpContext.Session.GetJson<List<CartModel>>("Cart") ?? new List<CartModel>();


				// Thêm chi tiết đơn hàng cho từng sản phẩm trong giỏ hàng
				foreach (var cart in cartsItems)
                {
					// Tìm sản phẩm trong cơ sở dữ liệu
					var product = await _dataContext.Products.FindAsync(cart.ProductId);

					// Kiểm tra nếu không đủ hàng trong kho
					if (product.Stock < cart.Quantity)
					{
						 TempData["ErrorMessage"] = $"Not enough stock for product {product.Name}. Only {product.Stock} remaining.";
						return RedirectToAction("Index", "Cart");
					}

					// Giảm số lượng hàng trong kho
					product.Stock -= cart.Quantity;

					// Đảm bảo đối tượng Product được cập nhật và theo dõi bởi DbContext
					_dataContext.Entry(product).State = EntityState.Modified;


					var orderDetails = new OrderDetails
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity
                    };

                    _dataContext.Add(orderDetails);
                }

                // Ghi log các thực thể đang được theo dõi
                foreach (var entry in _dataContext.ChangeTracker.Entries())
                {
                    _logger.LogInformation("Thực thể: {EntityName}, Trạng thái: {State}, Giá trị: {Values}",
                        entry.Entity.GetType().Name, entry.State,
                        entry.CurrentValues.Properties
                            .Select(p => $"{p.Name}: {entry.CurrentValues[p]}")
                            .Aggregate((a, b) => $"{a}, {b}"));
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _dataContext.SaveChangesAsync();

                // Xóa giỏ hàng sau khi đặt hàng thành công
                HttpContext.Session.Remove("Cart");

                // Gửi email thông báo thành công đến người dùng
                var subject = "Xác nhận đơn hàng thành công";
                var message = "Đơn hàng của bạn đã được xác nhận. Cảm ơn bạn đã mua hàng!";

                try
                {
                    await _emailSender.SendEmailAsync(userEmail, subject, message);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Gửi email xác nhận đơn hàng thất bại.");
                    // Không dừng quy trình thanh toán ngay cả khi gửi email thất bại
                }

                TempData["success"] = "Order created successfully!";
                return RedirectToAction("Index", "Cart");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi cập nhật cơ sở dữ liệu: {Message}. Lỗi bên trong: {InnerMessage}",
                    dbEx.Message, dbEx.InnerException?.Message);
                TempData["error"] = $"Lỗi cập nhật cơ sở dữ liệu: {dbEx.Message}. Lỗi bên trong: {dbEx.InnerException?.Message}";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi tạo đơn hàng: {Message}. Lỗi bên trong: {InnerMessage}",
                    ex.Message, ex.InnerException?.Message);
                TempData["error"] = $"Đã xảy ra lỗi: {ex.Message}. Lỗi bên trong: {ex.InnerException?.Message}";
                return RedirectToAction("Index", "Cart");
            }
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy tất cả các đơn hàng của người dùng từ cơ sở dữ liệu
                var orders = await _dataContext.Orders
                    .Where(o => o.UserName == userEmail)
                    .OrderByDescending(o => o.CreatedDate) // Sắp xếp theo ngày tạo
                    .ToListAsync();

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving order history: {Message}", ex.Message);
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                _logger.LogWarning("Order code is null or empty.");
                return BadRequest("Order code cannot be null or empty.");
            }

            var orderDetails = await _dataContext.OrderDetails
                                 .Include(od => od.Product)
                                 .Where(od => od.OrderCode == orderCode)
                                 .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound("No details found for this order.");
            }

            return View(orderDetails);
        }
       







    }
}




    
