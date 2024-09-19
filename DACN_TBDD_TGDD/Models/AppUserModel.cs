using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models
{
	public class AppUserModel : IdentityUser
	{

		public string RoleId { get; set; }
		public string Address { get; set; }

		[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? BirthDate { get; set; }


	}
}
