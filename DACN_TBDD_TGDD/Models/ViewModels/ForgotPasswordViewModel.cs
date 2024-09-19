using System.ComponentModel.DataAnnotations;

namespace DACN_TBDD_TGDD.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
