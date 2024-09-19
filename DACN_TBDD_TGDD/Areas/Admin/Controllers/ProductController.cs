using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Sales")] // Allow Admin and Sales to view
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        

        private async Task<string> DownloadImageAsync(string imageUrl, string savePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    await System.IO.File.WriteAllBytesAsync(savePath, imageBytes);
                    return savePath;
                }
                else
                {
                    // Handle error
                    return null;
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> Import(IFormFile fileUpload)
        {
            if (fileUpload == null || fileUpload.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "No file selected.");
                return View();
            }

            using (var stream = new MemoryStream())
            {
                await fileUpload.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Read data from Excel cells
                        var name = worksheet.Cells[row, 1].Text;
                        var description = worksheet.Cells[row, 2].Text;
                        var price = decimal.Parse(worksheet.Cells[row, 3].Text);
                        var categoryId = int.Parse(worksheet.Cells[row, 4].Text);
                        var brandId = int.Parse(worksheet.Cells[row, 5].Text);
                        var imageFileName = worksheet.Cells[row, 6].Text;
                        var slug = worksheet.Cells[row, 7].Text;
                        var stock = int.Parse(worksheet.Cells[row, 8].Text);

                        // Check for duplicate slugs 
                        var existingProduct = await _dataContext.Products
                            .FirstOrDefaultAsync(p => p.Slug == slug);
                        if (existingProduct != null)
                        {
                            continue; // Skip to the next row if a duplicate slug is found
                        }

                        // Create ProductModel (store just the image file name)
                        var product = new ProductModel
                        {
                            Name = name,
                            Description = description,
                            Price = price,
                            CategoryId = categoryId,
                            BrandId = brandId,
                            Slug = slug,
                            Image = imageFileName, // Store ONLY the file name
                            Stock = stock
                        };

                        _dataContext.Products.Add(product);
                    }

                    await _dataContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }


        // GET: /Admin/Product/Index
        [Authorize(Roles = "Admin,Sales")] // Allow Admin and Sales to view
        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _dataContext.Products
                .OrderByDescending(p => p.Id)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();

            const int pageSize = 5; // Number of items per page
            var paginatedProducts = new Paginate(products.Count, page, pageSize);

            var productsToDisplay = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pager = paginatedProducts; // Pass pagination info to the view
            return View(productsToDisplay); // Pass the paginated list of products
        }

        // GET: /Admin/Product/Create
        [Authorize(Roles = "Admin")] // Only Admin can access
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Create(ProductModel model)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandId);

            if (ModelState.IsValid)
            {
                model.Slug = model.Name.Replace(" ", "-").ToLower();
                var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == model.Slug);
                if (existingProduct != null)
                {
                    ModelState.AddModelError("", "Product already exists in the database.");
                    return View(model);
                }

                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Ensure directory exists
                    Directory.CreateDirectory(uploadDir);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fs);
                    }

                    model.Image = imageName;
                }

                _dataContext.Products.Add(model);
                await _dataContext.SaveChangesAsync();

                TempData["Success"] = "Product added successfully.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "There were some errors with your submission.";
            return View(model);
        }

        // GET: /Admin/Product/Edit/5
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Edit(long id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _dataContext.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(await _dataContext.Brands.ToListAsync(), "Id", "Name", product.BrandId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Edit(long id, ProductModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ViewBag.Categories = new SelectList(await _dataContext.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            ViewBag.Brands = new SelectList(await _dataContext.Brands.ToListAsync(), "Id", "Name", model.BrandId);

            var existedProduct = await _dataContext.Products.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                // Update slug
                model.Slug = model.Name.Replace(" ", "-").ToLower();

                // Handle image upload
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existedProduct.Image))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", existedProduct.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Upload new image
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Ensure directory exists
                    Directory.CreateDirectory(uploadsDir);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fs);
                    }

                    existedProduct.Image = imageName;
                }

                // Update other product details
                existedProduct.Name = model.Name;
                existedProduct.Description = model.Description;
                existedProduct.Price = model.Price;
                existedProduct.CategoryId = model.CategoryId;
                existedProduct.BrandId = model.BrandId;
                existedProduct.Slug = model.Slug;
                existedProduct.Stock= model.Stock;
                _dataContext.Products.Update(existedProduct);
                await _dataContext.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "There were some errors with your submission.";
            return View(model);
        }

        // GET: /Admin/Product/Delete/5
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> Delete(long id)
        {
            var product = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can access
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _dataContext.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Delete image if exists
            if (!string.IsNullOrEmpty(product.Image))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
		public async Task<IActionResult> Search(string searchTerm = "", int page = 1)
		{
			const int pageSize = 5; // Number of items per page
			var query = _dataContext.Products.AsQueryable();

			// Apply search filtering
			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.Name.Contains(searchTerm));
			}

			var totalProducts = await query.CountAsync(); // Get total number of products after filtering

			// Pagination logic
			var paginate = new Paginate(totalProducts, page, pageSize);

			var products = await query
								  .Include(p => p.Category)
								  .Include(p => p.Brand)
								  .Skip((page - 1) * pageSize)
								  .Take(pageSize)
								  .ToListAsync();

			ViewBag.Pager = paginate;
			ViewBag.SearchTerm = searchTerm;
			return View("Index", products); // Return to the Index view with filtered results
		}


	}
}
