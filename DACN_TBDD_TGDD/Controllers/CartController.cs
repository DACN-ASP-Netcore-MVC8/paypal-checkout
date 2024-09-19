using DACN_TBDD_TGDD.Areas.Admin.Repository;
using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.ViewModels;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal;
using PayPal.Api;
using System.Security.Claims;

namespace DACN_TBDD_TGDD.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<CartController> _logger;
        private readonly IEmailSender _emailSender;

        public CartController(DataContext dataContext, ILogger<CartController> logger, IEmailSender emailSender)
        {
            _dataContext = dataContext;
            _logger = logger;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart") ?? new List<CartModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = carts,
                GrandTotal = carts.Sum(x => x.Quantity * x.Price),
            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index");
        }

        public async Task<IActionResult> Add(long Id)
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to login page with the return URL as the current URL
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Headers["Referer"].ToString() });
            }

            // Fetch the product from the database
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Index", "Home");
            }

            // Check if stock is available
            if (product.Stock <= 0)
            {
                TempData["StockMessage"] = "Product is out of stock!";
                return RedirectToAction("Index", "Home");
            }

            // Get the cart from the session
            List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart") ?? new List<CartModel>();
            CartModel cartModel = carts.FirstOrDefault(c => c.ProductId == Id);

            // Add or update the product in the cart
            if (cartModel == null)
            {
                carts.Add(new CartModel(product));
                TempData["SuccessMessage"] = "Product added to cart successfully!";
            }
            else
            {
                // Ensure there is enough stock to update the quantity
                if (cartModel.Quantity >= product.Stock)
                {
                    TempData["ErrorMessage"] = "Not enough stock to update the quantity!";
                    return RedirectToAction("Index", "Home");
                }

                cartModel.Quantity += 1;
                TempData["SuccessMessage"] = "Product quantity updated!";
            }

            // Save the cart to the session
            HttpContext.Session.SetJson("Cart", carts);

            // Redirect back to the referring page or home if no referer is available
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(referer);
        }

        public async Task<IActionResult> Decrease(long Id)  // Changed int to long
        {
            List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart");
            CartModel cartModel = carts.FirstOrDefault(c => c.ProductId == Id);

            if (cartModel != null)
            {
                if (cartModel.Quantity > 1)
                {
                    --cartModel.Quantity;
                    TempData["  "] = "Product quantity decreased!";
                }
                else
                {
                    carts.RemoveAll(c => c.ProductId == Id);
                    TempData["QuantitySuccessMessage"] = "Product removed from cart!";
                }

                if (carts.Count == 0)
                {
                    HttpContext.Session.Remove("Cart");
                }
                else
                {
                    HttpContext.Session.SetJson("Cart", carts);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Product not found in cart!";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(long Id)  // Changed int to long
        {
            List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart");
            CartModel cartModel = carts.FirstOrDefault(c => c.ProductId == Id);
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			if (cartModel != null && product != null)
            {
				if (product.Stock <= cartModel.Quantity)
				{
					TempData["ErrorMessage"] = $"Not enough stock for product {product.Name}. Only {product.Stock} remaining.";
					return RedirectToAction("Index", "Cart");
				}
				++cartModel.Quantity;
                //TempData["QuantitySuccessMessage"] = "Product quantity increased!";
                HttpContext.Session.SetJson("Cart", carts);
            }
            else
            {
                TempData["ErrorMessage"] = "Product not found in cart!";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(long Id)  // Changed int to long
        {
			List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart");
			CartModel cartModel = carts.FirstOrDefault(c => c.ProductId == Id);

			if (cartModel != null)
			{
				carts.Remove(cartModel);

				if (carts.Count == 0)
				{
					HttpContext.Session.Remove("Cart");
				}
				else
				{
					HttpContext.Session.SetJson("Cart", carts);
				}

				TempData["SuccessMessageCart"] = "Product removed successfully!";
			}
			else
			{
				TempData["ErrorMessageCart"] = "Product not found in cart!";
			}

			return RedirectToAction("Index");
		}

        public async Task<IActionResult> Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["MessageClearCart"] = "Cart cleared successfully!";
            return RedirectToAction("Index");
        }
       
		public int GetCartItemCount()
		{
            // Retrieve the cart from session using a custom method GetJson
            List<CartModel> carts = HttpContext.Session.GetJson<List<CartModel>>("Cart");

            // Sum the quantity of all cart items if the cart exists, otherwise return 0
            return carts?.Sum(c => c.Quantity) ?? 0;
        }
        public ActionResult FailureView()
        {
            
            return View();
        }

        public ActionResult SuccessView()
        {
            
            return View();
        }

        public async Task<IActionResult> PaymentWithPaypal(string cancel = null, string guid = null)
        {
            var apiContext = PaypalConfiguration.GetAPIContext();

            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    return await CreatePaymentAndRedirect(apiContext);
                }
                else
                {
                    return await ExecutePaymentAndRedirect(apiContext, cancel, guid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PayPal payment: {Message}", ex.Message);
                TempData["error"] = "An error occurred during payment processing. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }

        private async Task<IActionResult> CreatePaymentAndRedirect(APIContext apiContext)
        {
            var cartItems = HttpContext.Session.GetJson<List<CartModel>>("Cart") ?? new List<CartModel>();
            if (!cartItems.Any())
            {
                TempData["error"] = "Cart is empty!";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cartItems.Sum(x => x.Price * x.Quantity);
            var returnUrl = Url.Action("PaymentWithPaypal", "Cart", new { guid = Guid.NewGuid() }, Request.Scheme);
            var payment = CreatePayment(apiContext, cartItems, totalAmount, returnUrl);

            var redirectUrl = payment.links.FirstOrDefault(x => x.rel.ToLower() == "approval_url")?.href;
            return Redirect(redirectUrl);
        }

        private async Task<IActionResult> ExecutePaymentAndRedirect(APIContext apiContext, string cancel, string guid)
        {
            var payerId = Request.Query["PayerID"];
            var paymentId = Request.Query["paymentId"];

            if (payerId.Count == 0 || paymentId.Count == 0)
            {
                return RedirectToAction("FailureView");
            }

            var payment = ExecutePayment(apiContext, payerId, paymentId);

            if (payment.state.ToLower() != "approved")
            {
                return RedirectToAction("FailureView");
            }

            // Payment approved, store order details in the database
            try
            {
                // Get user email from claim
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _dataContext.Users
           .FirstOrDefaultAsync(u => u.Email == userEmail);

                // Generate unique order code
                var orderCode = Guid.NewGuid().ToString();

                // Create a new order entry
                var order = new OrderModel
                {
                    OrderCode = orderCode,
                    UserName = userEmail,
                    Status = 3, // Assuming 1 is the status for successful payment
                    CreatedDate = DateTime.Now,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber
                };

                // Add the order to the database
                _dataContext.Orders.Add(order);

                // Get cart items from the session
                List<CartModel> cartItems = HttpContext.Session.GetJson<List<CartModel>>("Cart") ?? new List<CartModel>();

                // Add order details for each product in the cart
                foreach (var cartItem in cartItems)
                {
                    var orderDetail = new OrderDetails
                    {
                        OrderCode = orderCode,
                        UserName = userEmail,
                        ProductId = cartItem.ProductId,
                        Price = cartItem.Price,
                        Quantity = cartItem.Quantity,


                    };

                    _dataContext.OrderDetails.Add(orderDetail);
                }

                // Save changes to the database
                await _dataContext.SaveChangesAsync();

                // Clear the cart session after successful payment
                HttpContext.Session.Remove("Cart");
				decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);
				// Optionally send a confirmation email (if not done earlier)
				var subject = "Order Confirmation";
				var message = $"Thank you for your purchase! Your order has been confirmed. The total amount paid is {totalAmount:C}.";
				await _emailSender.SendEmailAsync(userEmail, subject, message);

                // Redirect to the success page
                TempData["success"] = "Payment Successful!";
                return RedirectToAction("SuccessView");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the order details: {Message}", ex.Message);
                TempData["error"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }
        private Payment CreatePayment(APIContext apiContext, List<CartModel> cartItems, decimal totalAmount, string returnUrl)
        {
            var payer = new Payer { payment_method = "paypal" };
            var redirectUrls = new RedirectUrls
            {
                cancel_url = $"{Request.Scheme}://{Request.Host}/Cart/PaymentWithPaypal?cancel=true",
                return_url = returnUrl
            };

            var details = new Details
            {
                tax = "0",
                shipping = "0",
                subtotal = totalAmount.ToString()
            };

            var amount = new Amount
            {
                currency = "USD",
                total = totalAmount.ToString(),
                details = details
            };

            var transactionList = new List<Transaction>
    {
        new Transaction
        {
            description = "Transaction description.",
            amount = amount,
            item_list = new ItemList
            {
                items = cartItems.Select(item => new Item
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = item.Price.ToString(),
                    quantity = item.Quantity.ToString()
                }).ToList()
            }
        }
    };

            var payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            return payment.Create(apiContext);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

    }




}


