using InstagramClone.Data.Entities;

namespace InstagramClone.Interfaces
{
	public interface IEmailSender
	{
		public Task SendAccountVerificationEmail(string email, string token);
		public Task SendPasswordResetEmail(string email, string token);
	}
}
