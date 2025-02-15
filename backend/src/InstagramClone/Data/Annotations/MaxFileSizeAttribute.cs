using System.ComponentModel.DataAnnotations;

namespace InstagramClone.Data.Annotations
{
	public class MaxFileSizeAttribute(long maxFileSizeInMB) : ValidationAttribute
	{
		private readonly long _maxFileSizeInMB = maxFileSizeInMB * 1024 * 1024;
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is null)
				return ValidationResult.Success;
			else if (value is IFormFile file && file.Length <= _maxFileSizeInMB)
				return ValidationResult.Success;
			else
				return new ValidationResult($"File is too big. (Max file size: {maxFileSizeInMB} MB)");
		}
	}
}
