using InstagramClone.Data.Entities;

namespace InstagramClone.Interfaces
{
	public interface IEmailSender
	{
		public Task SendAccountVerificationEmail(User user, string encodedToken);
	}
}
