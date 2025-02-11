using InstagramClone.DTOs.Authentication;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController(AuthService authService) : ControllerBase
	{
		private readonly AuthService _authService = authService;

		[HttpPost("register")]
		public async Task<IActionResult> RegisterUser(UserRegisterDTO userRegisterDTO)
		{
			var result = await _authService.RegisterUser(userRegisterDTO);
			if (!result.Succeeded)
				return this.ProblemWithErrors(statusCode: 400, errors: result.Errors);
			else
				return Created();
		}

		[HttpPost("send-email-verification")]
		public async Task<IActionResult> SendAccountVerificationEmail(string email)
		{
			var result = await _authService.SendAccountVerificationEmail(encodedEmail: email);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return NotFound();
				else
					return this.ProblemWithErrors(statusCode: 400, errors: result.Errors);
			}
			else
				return NoContent();
		}

		[HttpPost("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(string encodedEmail, string code)
		{
			var result = await _authService.ConfirmEmail(encodedEmail, code);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return NotFound();
				else
					return this.ProblemWithErrors(statusCode: 400, errors: result.Errors);
			}
			else
				return Ok();
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDTO userLoginDTO)
		{
			var (result, user) = await _authService.CheckLoginInfo(userLoginDTO);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return NotFound();
				else if (result.Errors.Any(e => e.Code == "EmailNotVerified"))
					return this.ProblemWithErrors(statusCode: 401, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, errors: result.Errors);
			}

			var token = _authService.GenerateToken(user);
			return Ok(new { token });
		}
	}
}
