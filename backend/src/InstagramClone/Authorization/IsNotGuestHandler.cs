using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InstagramClone.Authorization
{
	public class IsNotGuestHandler : AuthorizationHandler<IsNotGuestRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsNotGuestRequirement requirement)
		{
			ClaimsPrincipal user = context.User;
			if (!user.Identity?.IsAuthenticated ?? true)
				context.Fail();
			else if (user.FindFirstValue("username") == "guest")
				context.Fail(new AuthorizationFailureReason(this, "Guest is not allowed"));
			else
				context.Succeed(requirement);
			return Task.CompletedTask;
		}
	}
}
