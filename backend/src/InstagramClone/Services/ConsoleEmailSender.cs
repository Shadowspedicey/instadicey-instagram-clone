using InstagramClone.Interfaces;
using System.Net;

namespace InstagramClone.Services
{
	public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger, IConfiguration configuration) : IEmailSender
	{
		private readonly ILogger<ConsoleEmailSender> _logger = logger;
		private string _frontend = configuration["Frontend"] ?? throw new ArgumentException("Frontend origin missing.");
		public Task SendAccountVerificationEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);
			string verificationLink = $"{_frontend}/#/accounts/verify?mode=verifyEmail&user={encodedEmail}&token={encodedToken}";
			string emailContent = $"""
				=== Email Sent to Console ===
				To: {email}
				Subject: Instadicey email verification
				=============================
				Hi,
				
				Please verify your email by clicking the link below:
				{verificationLink}
				
				If you did not create this request, ignore this email.
				
				Best,
				Instadicey
				=============================
				""";
			_logger.LogInformation(emailContent);
			return Task.CompletedTask;
		}

		public Task SendPasswordResetEmail(string email, string token)
		{
			var encodedEmail = WebUtility.UrlEncode(email);
			var encodedToken = WebUtility.UrlEncode(token);

			string verificationLink = $"{_frontend}/#/accounts/verify?mode=resetPassword&user={encodedEmail}&token={encodedToken}";
			string emailContent = $"""
				=== Email Sent to Console ===
				To: {email}
				Subject: Instadicey password reset
				=============================
				Hi,

				Please reset your password by clicking the link below:
				{verificationLink}
				
				If you did not create this request, ignore this email.
				
				Best,
				Instadicey
				""";

			_logger.LogInformation(emailContent);
			return Task.CompletedTask;
		}
	}
}
