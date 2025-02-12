using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace InstagramClone.Data.Annotations
{
	public class ImageOnlyAttribute : ValidationAttribute
	{
		private readonly string[] allowedExtensions = { ".png", ".jpg", "jpeg" };
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (
					value is IFormFile file &&
					allowedExtensions.Contains(Path.GetExtension(file.FileName)) &&
					Regex.IsMatch(file.ContentType, "^image/")
				)
				return ValidationResult.Success;
			else
				return new ValidationResult("Photo field must be an image.");
		}
	}
}
