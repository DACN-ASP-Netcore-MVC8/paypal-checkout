using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models
{
	public class CategoryModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage ="Yeu cau nhap ten Danh Muc")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yeu cau nhap ten Danh Muc")]
		public string Description { get; set; }
		[Required]
		public string Slug { get; set; }
		
		public int status {  get; set; }
		
		

	}
}
