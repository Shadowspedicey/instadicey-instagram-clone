using InstagramClone.DTOs.Authentication;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static InstagramClone.Utils.ExtensionMethods;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController(AuthService authService) : ControllerBase
	{
		private readonly AuthService _authService = authService;
		private string DownloadFileEndpoint => $"{Request.Scheme}://{Request.Host}/file/";

		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[HttpPost("register")]
		public async Task<IActionResult> RegisterUser(UserRegisterDTO userRegisterDTO)
		{
			var result = await _authService.RegisterUser(userRegisterDTO);
			if (!result.Succeeded)
				return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			else
				return Created();
		}

		[SwaggerOperation(
			summary: "Sends an account verification email to the user.",
			description: @"This is the endpoint that is used right after registration.
- Returns 200 when user exists.
- Returns 400 when the data provided is invalid.
- Returns 404 when the email provided doesn't belong to a user."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("send-email-verification")]
		public async Task<IActionResult> SendAccountVerificationEmail(string email)
		{
			var result = await _authService.SendAccountVerificationEmail(encodedEmail: email);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return this.ProblemWithErrors(statusCode: 404, detail: "Email is invalid or user doesn't exist.", errors: new[] {new CodedError(ErrorCode.NotFound, "Email is invalid or user doesn't exist..").Metadata });
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}
			else
				return NoContent();
		}

		[SwaggerOperation(
			summary: "Confirms the verification of a user's account.",
			description: @"This is the endpoint that is used to confirm the account using the code sent over email.
- Returns 200 when user exists and code is valid.
- Returns 400 when any of the data provided is invalid.
- Returns 404 when the email provided doesn't belong to a user."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(string encodedEmail, string code)
		{
			var result = await _authService.ConfirmEmail(encodedEmail, code);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return this.ProblemWithErrors(statusCode: 404, detail: "Email is invalid or user doesn't exist.", errors: new[] {new CodedError(ErrorCode.NotFound, "Email is invalid or user doesn't exist.").Metadata });
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}
			else
				return Ok();
		}

		[SwaggerOperation(
	summary: "Sends an account email change confirmation email to the user.",
	description: @"This is the endpoint that is used to send a request email to change the email associated with the user's account.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 403 when the user is a guest."
)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(403)]
		[Authorize("IsNotGuest")]
		[HttpPost("send-email-change-verification")]
		public async Task<IActionResult> SendEmailChangeRequest(EmailChangeDTO emailChangeDTO)
		{
			var result = await _authService.SendEmailChangeRequest(User, emailChangeDTO.Email, emailChangeDTO.Password);
			if (!result.Succeeded)
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			return NoContent();
		}

		[SwaggerOperation(
		summary: "Confirms an email change for a user.",
		description: @"This is the endpoint that is used actually change the email using the code sent from the change request.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 403 when the user is a guest."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(403)]
		[Authorize("IsNotGuest")]
		[HttpPost("confirm-email-change")]
		public async Task<IActionResult> ConfirmEmailChange(string newEmail, string code)
		{
			var result = await _authService.ConfirmEmailChange(User, newEmail, code);
			if (!result.Succeeded)
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			return NoContent();
		}

		[SwaggerOperation(
		summary: "Changes the password of a user.",
		description: @"This is the endpoint that is used to change to a new password by providing the old password.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 403 when the user is a guest."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(403)]
		[Authorize("IsNotGuest")]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword(PasswordChangeDTO passwordChangeDTO)
		{
			var result = await _authService.ChangePassword(User, passwordChangeDTO.CurrentPassword, passwordChangeDTO.NewPassword);
			if (!result.Succeeded)
				return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			return NoContent();
		}

		[SwaggerOperation(
		summary: "Sends a password reset email.",
		description: @"This is the endpoint that is used to send a password reset email in case the user forgot their password.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 404 when the email provided is not associated with a user."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("send-password-reset")]
		public async Task<IActionResult> SendPasswordResetEmail(string email)
		{
			var result = await _authService.SendPasswordResetEmail(email);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == Enum.GetName(ErrorCode.NotFound)))
					return this.ProblemWithErrors(statusCode: 404, detail: result.Errors.First().Description, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}
			return NoContent();
		}

		[SwaggerOperation(
		summary: "Checks the validity of the password reset token.",
		description: @"This is the endpoint that is used to check the validity of the token sent over email, and if valid the user can procceed to change their password using the next endpoint.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 404 when the email provided is not associated with a user."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("check-password-reset-token")]
		public async Task<IActionResult> CheckPasswordResetToken(string email, string token)
		{
			var result = await _authService.CheckPasswordResetToken(email, token);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == Enum.GetName(ErrorCode.NotFound)))
					return this.ProblemWithErrors(statusCode: 404, detail: result.Errors.First().Description, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}
			return NoContent();
		}

		[SwaggerOperation(
		summary: "Resets a user's passowrd.",
		description: @"This is the endpoint that is used to reset a user's password.
- Returns 200 when user exists and data is valid.
- Returns 400 when the data provided is invalid.
- Returns 404 when the email provided is not associated with a user."
		)]
		[ProducesResponseType(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("password-reset")]
		public async Task<IActionResult> ResetPassword(PasswordResetDTO passwordResetDTO)
		{
			var result = await _authService.ResetPassword(passwordResetDTO.Email, passwordResetDTO.Token, passwordResetDTO.NewPassword);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == Enum.GetName(ErrorCode.NotFound)))
					return this.ProblemWithErrors(statusCode: 404, detail: result.Errors.First().Description, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}
			return NoContent();
		}

		[SwaggerOperation(
		description: @"This is the endpoint that is used for login.
- Returns 200 when user exists and data is valid.
- Returns 400 when the email/password provided is invalid.
- Returns 401 when the user hasn't verified his email yet.
- Returns 404 when the email provided is not associated with a user."
		)]
		[ProducesResponseType<LoginSuccessDTO>(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(401)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDTO userLoginDTO)
		{
			var (result, user) = await _authService.CheckLoginInfo(userLoginDTO);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "UserNotFound"))
					return this.ProblemWithErrors(statusCode: 404, detail: result.Errors.First().Description, errors: result.Errors);
				else if (result.Errors.Any(e => e.Code == "EmailNotVerified"))
					return this.ProblemWithErrors(statusCode: 401, detail: result.Errors.First().Description, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}

			var token = _authService.GenerateToken(DownloadFileEndpoint, user);
			var refreshToken = await _authService.GenerateRefreshToken(user!);
			return Ok(new LoginSuccessDTO() { Token = token, RefreshToken = refreshToken.Token});
		}

		[SwaggerOperation(
		summary: "Refresh token",
		description: @"This is the endpoint that is used to get new JWT tokens when they expire using refresh tokens.
- Returns 200 when the refresh token exists and is valid.
- Returns 400 when the refresh token provided is invalid.
- Returns 401 when the refresh token exists but is expired.
- Returns 404 when the token provided doesn't exist."
		)]
		[ProducesResponseType<LoginSuccessDTO>(200)]
		[ProducesResponseType<ProblemDetailsWithErrors>(400)]
		[ProducesResponseType<ProblemDetailsWithErrors>(401)]
		[ProducesResponseType<ProblemDetailsWithErrors>(404)]
		[HttpPost("login/refresh")]
		public async Task<IActionResult> RefreshToken(string rt)
		{
			var (result, user) = await _authService.RefreshToken(rt);
			if (!result.Succeeded)
			{
				if (result.Errors.Any(e => e.Code == "NotFound"))
					return this.ProblemWithErrors(statusCode: 404, detail: result.Errors.First().Description, errors: result.Errors);
				else if (result.Errors.Any(e => e.Code == "TokenExpired"))
					return this.ProblemWithErrors(statusCode: 401, detail: result.Errors.First().Description, errors: result.Errors);
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors.First().Description, errors: result.Errors);
			}

			var token = _authService.GenerateToken(DownloadFileEndpoint, user);
			var refreshToken = await _authService.GenerateRefreshToken(user!);
			return Ok(new { token, refreshToken = refreshToken.Token });
		}
	}
}
