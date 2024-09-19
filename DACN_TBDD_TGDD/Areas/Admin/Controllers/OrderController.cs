using DACN_TBDD_TGDD.Areas.Admin.Models.ViewModels;
using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.ViewModels;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Sales")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<OrderController> _logger;

        public OrderController(DataContext dataContext, ILogger<OrderController> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var orderList = await _dataContext.Orders.ToListAsync();

            const int pageSize = 5; // Number of items per page
            var paginatedOrders = new Paginate(orderList.Count, page, pageSize);

            var ordersToDisplay = orderList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pager = paginatedOrders;
            return View(ordersToDisplay);
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

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _dataContext.Orders.Remove(order);
            await _dataContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> UpdateOrderStatus(int status, string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                _logger.LogWarning("Order code is null or empty.");
                return Json(new { success = false, message = "Order code cannot be null or empty." });
            }

            try
            {
                var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
                if (order != null)
                {
                    order.Status = status;
                    await _dataContext.SaveChangesAsync();
                    return Json(new { success = true });
                }
                else
                {
                    _logger.LogWarning($"Order with code {orderCode} not found.");
                    return Json(new { success = false, message = "Order not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> RevenueToday()
        {
            var today = DateTime.Today;
            var startOfDay = today;
            var endOfDay = today.AddDays(1).AddTicks(-1);

            // Lọc các đơn hàng thành công
            var successfulOrders = await _dataContext.Orders
                .Where(o => o.Status == 3 && o.CreatedDate >= startOfDay && o.CreatedDate <= endOfDay)
                .ToListAsync();

            // Tính toán doanh thu tổng và số lượng sản phẩm đã bán
            var totalRevenue = 0m;
            var productSales = new Dictionary<long, (long SoldQuantity, int Stock)>();

            foreach (var order in successfulOrders)
            {
                var orderDetails = await _dataContext.OrderDetails
                    .Where(od => od.OrderCode == order.OrderCode)
                    .ToListAsync();

                foreach (var detail in orderDetails)
                {
                    if (!productSales.ContainsKey(detail.ProductId))
                    {
                        var product = await _dataContext.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            productSales[detail.ProductId] = (0, product.Stock);
                        }
                    }

                    var (soldQuantity, stock) = productSales[detail.ProductId];
                    productSales[detail.ProductId] = (soldQuantity + detail.Quantity, stock);

                    totalRevenue += detail.Price * detail.Quantity;
                }
            }

            // Convert to a view model or view bag
            var viewModel = new RevenueTodayViewModel
            {
                TotalRevenue = totalRevenue,
                OrderCount = successfulOrders.Count,
                ProductSales = productSales.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ProductSaleViewModel
                    {
                        SoldQuantity = kvp.Value.SoldQuantity,
                        Stock = kvp.Value.Stock,
                        ProductName = _dataContext.Products
                            .Where(p => p.Id == kvp.Key)
                            .Select(p => p.Name)
                            .FirstOrDefault()
                    })
            };

            return View(viewModel);
        }




    }
}
