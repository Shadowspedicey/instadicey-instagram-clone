using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace InstagramClone.Data.Annotations
{
	public class UsernameAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string username && username.Length > 0 && username.Length <= 20 && Regex.IsMatch(username, "/^[A-Za-z0-9]*$/"))
				return ValidationResult.Success;
			else return new ValidationResult("Username has to be a string with 20 or less English characters");
		}
	}
}
