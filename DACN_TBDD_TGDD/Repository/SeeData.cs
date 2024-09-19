using DACN_TBDD_TGDD.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DACN_TBDD_TGDD.Repository
{
	public class SeeData
	{
		public static void SeedingData(DataContext _context)
		{
			//_context.Database.Migrate();

			//// Kiểm tra và thêm dữ liệu vào danh mục nếu chưa có
			//if (!_context.Categories.Any())
			//{
			//	var smartPhoneCategory = new CategoryModel
			//	{
			//		Name = "Điện thoại di động",
			//		Slug = "dtdd",
			//		Description = "",
					
			//	};

			//	var laptopCategory = new CategoryModel
			//	{
			//		Name = "Laptop",
			//		Slug = "lt",
			//		Description = "",
					
			//	};

			//	_context.Categories.AddRange(smartPhoneCategory, laptopCategory);
			//	_context.SaveChanges();
			//}

			//// Kiểm tra và thêm dữ liệu vào thương hiệu nếu chưa có
			//if (!_context.Brands.Any())
			//{
			//	var appleBrand = new BrandModel
			//	{
			//		Name = "Apple",
			//		Description = "Apple brand",
			//		Slug = "apple",
			//		Status = 1
			//	};

			//	var samsungBrand = new BrandModel
			//	{
			//		Name = "Samsung",
			//		Description = "Samsung brand",
			//		Slug = "samsung",
			//		Status = 1
			//	};

			//	_context.Brands.AddRange(appleBrand, samsungBrand);
			//	_context.SaveChanges();
			//}

			//// Kiểm tra và thêm dữ liệu vào sản phẩm nếu chưa có
			//if (!_context.Products.Any())
			//{
			//	// Lấy các danh mục và thương hiệu đã được thêm vào
			//	var appleCategory = _context.Categories.FirstOrDefault(c => c.Slug == "apple");
			//	var samsungCategory = _context.Categories.FirstOrDefault(c => c.Slug == "samsung");

			//	var appleBrand = _context.Brands.FirstOrDefault(b => b.Slug == "apple");
			//	var samsungBrand = _context.Brands.FirstOrDefault(b => b.Slug == "samsung");

			//	var macbookProduct = new ProductModel
			//	{
			//		Name = "MacBook",
			//		Slug = "macbook",
			//		Description = "MacBook is the best",
			//		Price = 12.2m,
			//		Category = appleCategory,
			//		Brand = appleBrand,
			//		Image = "1.jpg"
			//	};

			//	var galaxyProduct = new ProductModel
			//	{
			//		Name = "Galaxy",
			//		Slug = "galaxy",
			//		Description = "Galaxy is powerful",
			//		Price = 20.5m,
			//		Category = samsungCategory,
			//		Brand = samsungBrand,
			//		Image = "2.jpg"
			//	};

			//	_context.Products.AddRange(macbookProduct, galaxyProduct);
			//	_context.SaveChanges();
			//}
		}
	}
}
