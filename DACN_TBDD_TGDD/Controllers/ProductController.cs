using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.ViewModels;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DACN_TBDD_TGDD.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;

		public ProductController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			const int pageSize = 5; // Number of items per page
			var totalProducts = await _dataContext.Products.CountAsync(); // Get total number of products

			var paginate = new Paginate(totalProducts, page, pageSize);

			var products = await _dataContext.Products
											 .Skip((page - 1) * pageSize)
											 .Take(pageSize)
											 .ToListAsync();

			ViewBag.Pager = paginate;
			return View(products);
		}


		public async Task<IActionResult> Details(long Id )
		{
            // Check if the Id is valid (though as a long, you don't need a null check)
            if (Id <= 0)
            {
                return RedirectToAction("Index");
            }

            // Fetch the product by Id including its ratings
            var product = await _dataContext.Products
                                            .Include(p => p.Ratings)
                                            .FirstOrDefaultAsync(p => p.Id == Id);
            var ratings = await _dataContext.Ratings
                                .Where(r => r.ProductId == product.Id)
                                .ToListAsync();

            // Check if the product was found
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            // Map the ProductModel to the ViewModel (assuming you need to use ProductDetailsViewModel)
            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = product,
                ProductReviews = ratings

            };

            // Return the view with the correct ViewModel
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(RatingModel model)
        {
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = model.ProductId,
                    Name = model.Name,
                    Email = model.Email,
                    Comment = model.Comment,
                    Start = model.Start
                };

                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Review added successfully!";
                return RedirectToAction("Details", new { id = model.ProductId });
            }
            else
            {
                TempData["error"] = "There was an error submitting your review. Please check your information.";
                return RedirectToAction("Details", new { id = model.ProductId });
            }
        }




    }
}
