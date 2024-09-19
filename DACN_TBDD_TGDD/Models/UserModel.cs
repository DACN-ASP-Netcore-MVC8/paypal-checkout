using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models
{
	public class UserModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Please enter a username")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Please enter an email address"), EmailAddress]
		public string Email { get; set; }

		[DataType(DataType.Password), Required(ErrorMessage = "Please enter a password")]
		public string Password { get; set; }
	}
}
