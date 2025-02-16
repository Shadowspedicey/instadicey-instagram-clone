using InstagramClone.DTOs.Profile;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InstagramClone.Filters
{
	public class EditDTOUsernameLowercaseFilter(string dtoParameterName = "userData") : Attribute, IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var input = context.ActionArguments[dtoParameterName] as UserEditDTO;
			if (input is not null)
				input.Username = input.Username.ToLower();
			await next();
		}
	}
}
