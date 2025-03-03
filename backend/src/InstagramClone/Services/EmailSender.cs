using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace InstagramClone.Services
{
	public class EmailSender : IEmailSender
	{
		private readonly SendGridClient _emailClient;
		private readonly string _frontend;
		public EmailSender(IConfiguration configuration)
		{
			var apiKey = configuration["SendGrid:Key"];
			_emailClient = new SendGridClient(apiKey);
			_frontend = configuration["Frontend"] ?? throw new ArgumentException("Frontend origin missing.");
		}

		public async Task SendAccountVerificationEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);
			string verificationLink = $"{_frontend}/#/accounts/verify?mode=verifyEmail&user={encodedEmail}&token={encodedToken}";

			SendGridMessage message = new()
			{
				From = new EmailAddress("shadowspediceyapi@gmail.com", "Instadicey"),
				Subject = "Instadicey email verification",
				PlainTextContent = $"Hi,\n\nPlease verify your email by clicking the link below:\n{verificationLink}\n\nIf you did not create this request, ignore this email.\n\nBest,\nInstadicey",
				HtmlContent =
					$@"
					<h2>Verify Your Email</h2>
					<p>Hi,</p>
					<p>Click the button below to verify your email:</p>
					<a href='{verificationLink}' style='display:inline-block;margin:10px 0;padding:10px 20px;color:black;font-weight: bold;background-color:red;text-decoration:none;border-radius:5px;'>Verify Email</a>
					<p>If the button doesn't work, copy and paste this link into your browser:</p>
					<p>{verificationLink}</p>
					<br>
					<p>Best,<br>Instadicey</p>"
			};
			message.AddTo(new EmailAddress(email));

			var response = await _emailClient.SendEmailAsync(message);
		}

		public async Task SendPasswordResetEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);

			string verificationLink = $"{_frontend}/#/accounts/verify?mode=resetPassword&user={encodedEmail}&token={encodedToken}";

			SendGridMessage message = new()
			{
				From = new EmailAddress("shadowspediceyapi@gmail.com", "Instadicey"),
				Subject = "Instadicey password reset",
				PlainTextContent = $"Hi,\n\nPlease reset your password by clicking the link below:\n{verificationLink}\n\nIf you did not create this request, ignore this email.\n\nBest,\nInstadicey",
				HtmlContent =
					$@"
					<h2>Password Reset</h2>
					<p>Hi,</p>
					<p>Click the button below to reset your password:</p>
					<a href='{verificationLink}' style='display:inline-block;margin:10px 0;padding:10px 20px;color:black;font-weight: bold;background-color:red;text-decoration:none;border-radius:5px;'>Reset Password</a>
					<p>If the button doesn't work, copy and paste this link into your browser:</p>
					<p>{verificationLink}</p>
					<br>
					<p>Best,<br>Instadicey</p>"
			};
			message.AddTo(new EmailAddress(email));

			 await _emailClient.SendEmailAsync(message);
		}
	}
}
