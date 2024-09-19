    using DACN_TBDD_TGDD.Repository.Components.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DACN_TBDD_TGDD.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        [MinLength(4, ErrorMessage = "Tên sản phẩm phải có ít nhất 4 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        [MinLength(4, ErrorMessage = "Mô tả sản phẩm phải có ít nhất 4 ký tự")]
        public string Description { get; set; }

        public string Slug { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải là số dương")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100")]
        public float Discount { get; set; }

        public int BrandId { get; set; }
        public int CategoryId { get; set; }

        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }

        public RatingModel Ratings { get; set; }

        public string Image { get; set; }

        [NotMapped]
        [FileExtension("jpg", "png", "jpeg")]
        public IFormFile? ImageUpload { get; set; }

        public int Stock { get; set; }

    }
}
