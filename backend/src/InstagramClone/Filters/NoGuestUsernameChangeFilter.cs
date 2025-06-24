using InstagramClone.DTOs.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace InstagramClone.Filters
{
	public class NoGuestUsernameChangeFilter : Attribute, IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var input = context.ActionArguments["userData"] as UserEditDTO;
			if (context.HttpContext.User.FindFirstValue("username") == "guest" && input.Username != "guest")
				context.Result = new ForbidResult();
			else
				await next();
		}
	}
}
