using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace InstagramClone.Data.Annotations
{
	public class EnglishOnlyAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string v && Regex.IsMatch(v, "/^[A-Za-z0-9]*$/"))
				return ValidationResult.Success;
			return new ValidationResult("Only English letters allowed");
		}
	}
}
