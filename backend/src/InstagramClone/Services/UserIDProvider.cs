using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class UserIDProvider : IUserIdProvider
	{
		public string? GetUserId(HubConnectionContext connection) => connection.User.FindFirstValue("sub");
	}
}
