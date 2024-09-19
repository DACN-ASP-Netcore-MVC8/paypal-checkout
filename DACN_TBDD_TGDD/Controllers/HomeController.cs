using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DACN_TBDD_TGDD.Controllers
{
    
    public class HomeController : Controller
    {
		private readonly DataContext _dataContext;
		private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _dataContext = context;
        }

		public async Task<IActionResult> Index(int page = 1, string sortOrder = "")
		{
			const int pageSize = 9;
			var totalProducts = await _dataContext.Products.CountAsync();

			// Apply sorting based on sortOrder parameter
			IQueryable<ProductModel> products = _dataContext.Products;
			switch (sortOrder)
			{
				case "price_asc":
					products = products.OrderBy(p => p.Price);
					break;
				case "price_desc":
					products = products.OrderByDescending(p => p.Price);
					break;
				default:
					// Default sorting (e.g., by name or ID)
					products = products.OrderBy(p => p.Name);
					break;
			}

			var paginate = new Paginate(totalProducts, page, pageSize);

			products = products
				.Skip((page - 1) * pageSize)
				.Take(pageSize);

			ViewBag.Pager = paginate;
			ViewBag.CurrentSort = sortOrder; // Store the current sort order for view logic

			return View(await products.ToListAsync());
		}

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		public async Task<IActionResult> Search(string searchTerm = "", int page = 1)
		{
			const int pageSize = 5; // Number of items per page
			var query = _dataContext.Products.AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.Name.Contains(searchTerm));
			}

			var totalProducts = await query.CountAsync(); // Get total number of products after filtering

			var paginate = new Paginate(totalProducts, page, pageSize);

			var products = await query
								  .Skip((page - 1) * pageSize)
								  .Take(pageSize)
								  .ToListAsync();

			ViewBag.Pager = paginate;
			ViewBag.SearchTerm = searchTerm;
			return View("Index", products); // Return to the Index view with filtered results
		}
		public IActionResult Contact()
		{

			return View();
		}
        public IActionResult Chat(string receiverId)
        {
            // Pass the receiverId to the view
            ViewBag.ReceiverId = receiverId;
            return View();
        }



    }
}
