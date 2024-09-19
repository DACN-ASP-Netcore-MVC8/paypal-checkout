using DACN_TBDD_TGDD.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RatingModel
{
	[Key]
	public int Id { get; set; }

	[Required(ErrorMessage = "Product ID is required.")]
	public long ProductId { get; set; }

	[Required(ErrorMessage = "Comment is required.")]
	[StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
	public string Comment { get; set; }

	[Required(ErrorMessage = "Name is required.")]
	[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
	public string Name { get; set; }

	[Required(ErrorMessage = "Email is required.")]
	[EmailAddress(ErrorMessage = "Invalid email address.")]
	[StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
	public string Email { get; set; }

	[Required(ErrorMessage = "Rating is required.")]
	[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
	public int Start { get; set; }

	[ForeignKey("ProductId")]
	public ProductModel Product { get; set; }
}
