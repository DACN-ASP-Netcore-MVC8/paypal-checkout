using System.Net.Mail;
using System.Net;

namespace DACN_TBDD_TGDD.Areas.Admin.Repository
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string message)
		{
			var client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true, 
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential("thienngo2003@gmail.com", "mbkncreiekkykrqn")
			};

			return client.SendMailAsync(
				new MailMessage(from: "thienngo2003@gmail.com",
								to: email,
								subject,
								message
								));
		}
	}
}
