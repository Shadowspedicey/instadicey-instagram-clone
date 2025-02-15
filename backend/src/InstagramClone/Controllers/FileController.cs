using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class FileController(IFileService fileService) : ControllerBase
	{
		private readonly IFileService _fileService = fileService;

		[HttpGet("{encryptedFilePath}")]
		public async Task<IActionResult> GetFile(string encryptedFilePath)
		{
			string decryptedFilePath;
			try
			{
				decryptedFilePath = Helpers.Encryption.Decrypt(encryptedFilePath);
			}
			catch (ArgumentException)
			{
				return Problem(statusCode: 400, detail: "The format of the file request is invalid.");
			}

			var result = await _fileService.GetFile(decryptedFilePath);
			if (!result.IsSuccess)
				if (result.HasCodedErrorWithCode(ErrorCode.NotFound))
					return NotFound();
				else
					return this.ProblemWithErrors(statusCode: 400, detail: result.Errors[0].Message, errors: result.Errors?.Select(e => e?.Metadata)!);

			var fileName = Path.GetFileName(decryptedFilePath);
			var fileStream = result.Value;
			string contentType = Helpers.Files.GetMimeTypeForFileExtension(fileName);
			return new FileStreamResult(fileStream, contentType);
		}
	}
}
