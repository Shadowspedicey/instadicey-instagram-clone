using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace InstagramClone.Utils
{
	public static class Helpers
	{
		public static ObjectResult ProblemWithErrors(this ControllerBase controllerBase, object errors, int statusCode, string? detail = null)
		{
			return controllerBase.Problem(
				statusCode: statusCode,
				extensions: new Dictionary<string, object?> { { "errors", errors } },
				detail: detail
			);
		}
	}
}
