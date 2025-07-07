using InstagramClone.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net;

namespace InstagramClone.Services
{
	public class EmailSender : IEmailSender, IDisposable
	{
		private readonly SmtpClient _emailClient;
		private readonly string _frontend;
		public EmailSender(IConfiguration configuration)
		{
			var apiKey = configuration["SMTP:Key"];
			_emailClient = new();
			_emailClient.Connect("smtp.gmail.com", 587, false);
			_emailClient.Authenticate("shadowspediceyapi@gmail.com", apiKey);
			_frontend = configuration["Frontend"] ?? throw new ArgumentException("Frontend origin missing.");
		}

		public async Task SendAccountVerificationEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);
			string verificationLink = $"{_frontend}/#/accounts/verify?mode=verifyEmail&user={encodedEmail}&token={encodedToken}";

			MimeMessage message = new()
			{
				From = { new MailboxAddress("Instadicey", "shadowspedicey@gmail.com") },
				To = { new MailboxAddress(email, email) },
				Subject = "Instadicey email verification",
			};
			var plainText = new TextPart("plain")
			{
				Text = $"Hi,\n\nPlease verify your email by clicking the link below:\n{verificationLink}\n\nIf you did not create this request, ignore this email.\n\nBest,\nInstadicey"
			};
			var htmlText = new TextPart("html")
			{
				Text =
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
			var body = new Multipart("alternative")
			{
				plainText,
				htmlText
			};
			message.Body = body;

			await _emailClient.SendAsync(message);
		}

		public async Task SendPasswordResetEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);

			string verificationLink = $"{_frontend}/#/accounts/verify?mode=resetPassword&user={encodedEmail}&token={encodedToken}";

			MimeMessage message = new()
			{
				From = { new MailboxAddress("Instadicey", "shadowspediceyapi@gmail.com") },
				To = { new MailboxAddress(email, email) },
				Subject = "Instadicey password reset",
			};
			var plainText = new TextPart("plain")
			{
				Text = $"Hi,\n\nPlease reset your password by clicking the link below:\n{verificationLink}\n\nIf you did not create this request, ignore this email.\n\nBest,\nInstadicey"
			};
			var htmlText = new TextPart("html")
			{
				Text =
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
			var body = new Multipart("alternative")
			{
				plainText,
				htmlText
			};
			message.Body = body;

			await _emailClient.SendAsync(message);
		}

		public void Dispose()
		{
			_emailClient.Disconnect(true);
			_emailClient.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
