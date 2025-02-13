using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class FileController(IConfiguration configuration, IFileService fileService) : ControllerBase
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
				return BadRequest();
			}

			var result = await _fileService.GetFile(decryptedFilePath);
			if (!result.IsSuccess)
				if (result.HasError(e => e.Message == "NotFound"))
					return NotFound();
				else
					return Problem(statusCode: 400, detail: result.Errors?[0]?.Message);

			var fileName = Path.GetFileName(decryptedFilePath);
			var fileStream = result.Value;
			string contentType = Helpers.Files.GetMimeTypeForFileExtension(fileName);
			return new FileStreamResult(fileStream, contentType);
		}
	}
}
