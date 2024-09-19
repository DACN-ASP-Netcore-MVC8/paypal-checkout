using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace DACN_TBDD_TGDD.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;

		public BrandController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		public async Task<IActionResult> Index(string Slug = "")
		{
			BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
			if (brand == null) return RedirectToAction("Index");

			var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
			return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
