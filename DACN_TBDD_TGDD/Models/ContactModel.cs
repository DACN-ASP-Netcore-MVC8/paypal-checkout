using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_TBDD_TGDD.Models
{
	public class ContactModel
	{
		[Key]
		[Required(ErrorMessage = "Yêu cầu nhập tên.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập bản đồ.")]
		public string Map { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập email.")]
		[EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập mô tả.")]
		public string Description { get; set; }

		public string LogoImg { get; set; }

		[NotMapped]
		[FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Chỉ chấp nhận các định dạng ảnh (.jpg, .jpeg, .png, .gif).")]
		public IFormFile? ImageUpload { get; set; }


	}
}
