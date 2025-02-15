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
		public static IActionResult AppropriateResponseBasedOnResult(this ControllerBase controllerBase, Result result)
		{
			if (result.Errors.Count > 1)
				return controllerBase.ProblemWithErrors(statusCode: 400, errors: result.Errors.Select(e => e.Metadata));

			IError error = result.Errors[0];
			switch (error.Metadata["code"])
			{
				case ErrorCode.NotFound:
					return controllerBase.ProblemWithErrors(statusCode: 404, detail: error.Message, errors: error.Metadata);
				case ErrorCode.InsufficientPermissions:
					return controllerBase.ProblemWithErrors(statusCode: 403, detail: error.Message, errors: error.Metadata);
				case ErrorCode.Duplicate:
				case ErrorCode.InvalidInput:
				default:
					return controllerBase.ProblemWithErrors(statusCode: 400, detail: error.Message, errors: error.Metadata);
			}
		}
		public static IActionResult AppropriateResponseBasedOnResult<T>(this ControllerBase controllerBase, Result<T> result)
		{
			if (result.Errors.Count > 1)
				return controllerBase.ProblemWithErrors(statusCode: 400, errors: result.Errors.Select(e => e.Metadata));

			IError error = result.Errors[0];
			switch (Enum.Parse<ErrorCode>(error.Metadata["code"].ToString()!))
			{
				case ErrorCode.NotFound:
					return controllerBase.ProblemWithErrors(statusCode: 404, detail: error.Message, errors: error.Metadata);
				case ErrorCode.InsufficientPermissions:
					return controllerBase.ProblemWithErrors(statusCode: 403, detail: error.Message, errors: error.Metadata);
				case ErrorCode.Duplicate:
				case ErrorCode.InvalidInput:
				default:
					return controllerBase.ProblemWithErrors(statusCode: 400, detail: error.Message, errors: error.Metadata);
			}
		}

		public static bool HasCodedErrorWithCode<T>(this Result<T> result, ErrorCode errorCode) => result.HasError((CodedError e) => e?.Metadata?["code"] == Enum.GetName(errorCode));
		public static bool HasCodedErrorWithCode(this Result result, ErrorCode errorCode) => result.HasError((CodedError e) => e?.Metadata?["code"] == Enum.GetName(errorCode));
	}
}
