using FluentResults;

namespace InstagramClone.Utils
{
	public class CodedError : Error
	{
		public CodedError(ErrorCode code, string message) : base(message)
		{
			Metadata["code"] = Enum.GetName(code)!;
			Metadata["description"] = message;
		}
	}
}
