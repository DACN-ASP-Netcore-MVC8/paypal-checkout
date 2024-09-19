namespace DACN_TBDD_TGDD.Areas.Admin.Repository
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string email, string subject, string message); 
	}
}
