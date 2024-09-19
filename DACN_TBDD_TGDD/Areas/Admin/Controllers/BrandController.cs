using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;

namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "Admin,Sales")] // Allow Admin and Sales to view
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: Display the list of brands
        [Route("Index")]
        [Authorize(Roles = "Admin,Sales")] // Allow Admin and Sales to view
        public async Task<IActionResult> Index(int page = 1)
        {
            var brandList = await _dataContext.Brands.ToListAsync(); // Get all categories

            const int pageSize = 5; // Number of items per page
            var paginatedCategories = new Paginate(brandList.Count, page, pageSize);

            var categoriesToDisplay = brandList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pager = paginatedCategories; // Pass pagination info to the view
            return View(categoriesToDisplay);
        }

        // GET: Show the Create brand form
        [Route("Create")]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create a new brand
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Create(BrandModel model)
        {
            if (ModelState.IsValid)
            {
                // Create slug from brand name
                model.Slug = model.Name.Replace(" ", "-").ToLower();

                // Check if the slug already exists in the database
                var slugExists = await _dataContext.Brands
                    .FirstOrDefaultAsync(b => b.Slug == model.Slug);

                if (slugExists != null)
                {
                    ModelState.AddModelError("", "Brand with this name already exists.");
                    return View(model);
                }

                // Add the new brand to the database
                _dataContext.Add(model);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Brand created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "There was an error in the form submission.";
            return View(model);
        }

        // GET: Show the Edit brand form
        [Route("Edit/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Edit a brand
        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Edit(int id, BrandModel model)
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
                    _dataContext.Brands.Update(model);
                    await _dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Brand updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "There was an error in the form submission.";
            return View(model);
        }

        // GET: Show the Delete confirmation page
        [Route("Delete/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Brand deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete a brand
        [Route("DeleteConfirmed")]
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand != null)
            {
                _dataContext.Brands.Remove(brand);
                await _dataContext.SaveChangesAsync();
            }
            TempData["success"] = "Brand deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Check if a brand exists
        private bool BrandExists(int id)
        {
            return _dataContext.Brands.Any(b => b.Id == id);
        }
    }
}
