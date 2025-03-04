using InstagramClone.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InstagramClone.Authorization
{
	public class IsInChatRoomHandler : AuthorizationHandler<IsInChatRoomRequirement, ChatRoom>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsInChatRoomRequirement requirement, ChatRoom resource)
		{
			if (resource.Users.Any(u => u.Id == context.User.FindFirstValue("sub")))
				context.Succeed(requirement);
			return Task.CompletedTask;
		}
	}
}
