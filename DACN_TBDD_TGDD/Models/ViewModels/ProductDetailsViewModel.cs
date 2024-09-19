using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models.ViewModels
{
	public class ProductDetailsViewModel
	{
        public ProductModel ProductDetails { get; set; }

        // Properties for the Review Form (use the same names as in RatingModel)
        [Required(ErrorMessage = "Your name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Your email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "A rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Start { get; set; }
        public List<RatingModel> ProductReviews { get; set; }

    }
}
