using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Utils
{
	public static class ExtensionMethods
	{
		public static ObjectResult ProblemWithErrors(this ControllerBase controllerBase, object errors, int statusCode, string? detail = null)
		{
			return controllerBase.Problem(
				statusCode: statusCode,
				extensions: new Dictionary<string, object?> { { "errors", errors } },
				detail: detail
			);
		}

		public static bool HasCodedErrorWithCode<T>(this Result<T> result, ErrorCode errorCode) => result.HasError((CodedError e) => e?.Metadata?["code"] == Enum.GetName(errorCode));
		public static bool HasCodedErrorWithCode(this Result result, ErrorCode errorCode) => result.HasError((CodedError e) => e?.Metadata?["code"] == Enum.GetName(errorCode));
	}
}
