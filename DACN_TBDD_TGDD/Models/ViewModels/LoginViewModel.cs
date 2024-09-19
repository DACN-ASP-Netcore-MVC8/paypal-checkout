using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models.ViewModels
{
	public class LoginViewModel
	{

		public int Id { get; set; }

		[Required(ErrorMessage = "Please enter a username")]
		public string UserName { get; set; }

		[DataType(DataType.Password), Required(ErrorMessage = "Please enter a password")]
		public string Password { get; set; }

		public string ReturnUrl { get; set; }
	}
}
