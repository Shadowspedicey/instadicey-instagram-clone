using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;
using System.Net;

namespace InstagramClone.Services
{
	public class EmailSender : IEmailSender
	{
		private readonly SendGridClient _emailClient;
		public EmailSender(IConfiguration configuration)
		{
			var apiKey = configuration["SendGrid:Key"];
			_emailClient = new SendGridClient(apiKey);
		}
		public async Task SendAccountVerificationEmail(User user, string encodedToken)
		{
			var encodedEmail = WebUtility.UrlEncode(user.Email);
			string verificationLink = $"https://shadowspedicey.github.io/instagram-clone-fullstack/accounts/verify?user={encodedEmail}&token={encodedToken}";

			SendGridMessage message = new()
			{
				From = new EmailAddress("shadowspediceyapi@gmail.com", "Instadicey"),
				Subject = "Instadicey email verification",
				PlainTextContent = $"Hi {user.UserName},\n\nThank you for signing up! Please verify your email by clicking the link below:\n{verificationLink}\n\nIf you did not create this account, ignore this email.\n\nBest,\nInstadicey",
				HtmlContent =
					$@"
					<h2>Verify Your Email</h2>
					<p>Hi <strong>{user.UserName}</strong>,</p>
					<p>Click the button below to verify your email:</p>
					<a href='{verificationLink}' style='display:inline-block;padding:10px 20px;color:white;background-color:#007bff;text-decoration:none;border-radius:5px;'>Verify Email</a>
					<p>If the button doesn't work, copy and paste this link into your browser:</p>
					<p>{verificationLink}</p>
					<p>Best,<br>Instadicey</p>"
			};
			message.AddTo(new EmailAddress(user.Email));

			var response = await _emailClient.SendEmailAsync(message);
		}
	}
}
