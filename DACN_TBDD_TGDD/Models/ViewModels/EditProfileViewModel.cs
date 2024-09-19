using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models.ViewModels
{
	public class EditProfileViewModel
	{
		public string UserName { get; set; }

		[EmailAddress]
		public string Email { get; set; }

		[Phone]
		public string PhoneNumber { get; set; }

		public string Address { get; set; } // Add Address field

		[DataType(DataType.Date)]
		public DateTime? BirthDate { get; set; } // Add BirthDate field
	}
}
