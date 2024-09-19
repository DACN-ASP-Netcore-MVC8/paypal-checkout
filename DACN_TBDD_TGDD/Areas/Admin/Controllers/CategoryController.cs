using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin,Sales")] // Allow Admin and Sales to view
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;

		public CategoryController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		// GET: Display list of categories with pagination
		[HttpGet]
		public async Task<IActionResult> Index(int page = 1)
		{
            var categoryList = await _dataContext.Categories.ToListAsync(); // Get all categories

            const int pageSize = 5; // Number of items per page
            var paginatedCategories = new Paginate(categoryList.Count, page, pageSize);

            var categoriesToDisplay = categoryList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pager = paginatedCategories; // Pass pagination info to the view
            return View(categoriesToDisplay); // Pass the paginated list of categorieso the view // Pass the paginated list of categories
        }

		// GET: Create new category
		[Authorize(Roles = "Admin")] // Only Admin can access
		public IActionResult Create()
		{
			return View();
		}

		// POST: Create new category
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")] // Only Admin can access
		public async Task<IActionResult> Create(CategoryModel model)
		{
			if (ModelState.IsValid)
			{
				model.Slug = model.Name.Replace(" ", "-").ToLower();
				var slugExists = await _dataContext.Categories
					.FirstOrDefaultAsync(p => p.Slug == model.Slug);

				if (slugExists != null)
				{
					ModelState.AddModelError("", "Category with this name already exists.");
					return View(model);
				}

				_dataContext.Add(model);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Category created successfully.";
				return RedirectToAction("Index");
			}

			return View(model);
		}

		// GET: Edit category
		[Authorize(Roles = "Admin")] // Only Admin can access
		public async Task<IActionResult> Edit(long id)
		{
			var category = await _dataContext.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}

		// POST: Edit category
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")] // Only Admin can access
		public async Task<IActionResult> Edit(long id, CategoryModel model)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (ModelState.IsValid)
			{
				try
				{
					model.Slug = model.Name.Replace(" ", "-").ToLower();
					_dataContext.Categories.Update(model);
					await _dataContext.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CategoryExists(model.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}

		// GET: Confirm deletion of category
		[Authorize(Roles = "Admin")] // Only Admin can access
		public async Task<IActionResult> Delete(long id)
		{
			var category = await _dataContext.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		// POST: Delete category
		[HttpPost, ActionName("DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")] // Only Admin can access
		public async Task<IActionResult> DeleteConfirmed(long id)
		{
			var category = await _dataContext.Categories.FindAsync(id);
			if (category != null)
			{
				_dataContext.Categories.Remove(category);
				await _dataContext.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// Check if a category exists
		private bool CategoryExists(long id)
		{
			return _dataContext.Categories.Any(e => e.Id == id);
		}
	}
}
