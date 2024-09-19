using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models
{
	public class BrandModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MinLength(4, ErrorMessage = "Yeu cau nhap ten Thuong Hieu")]
		public string Name { get; set; }
		[Required, MinLength(4, ErrorMessage = "Yeu cau nhap ten Danh Muc")]
		public string Description { get; set; }
		public string Slug {  get; set; }
		public int Status {  get; set; }
	}
}
