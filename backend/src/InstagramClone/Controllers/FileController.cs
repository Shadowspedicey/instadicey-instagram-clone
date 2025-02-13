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
			var result = await _fileService.GetFile(encryptedFilePath);
			if (!result.IsSuccess)
				if (result.HasError(e => e.Message == "NotFound"))
					return NotFound();
				else
					return Problem(statusCode: 400, detail: result.Errors?[0]?.Message);

			var (fileStream, fileName) = result.Value;
			string contentType = Helpers.Files.GetMimeTypeForFileExtension(fileName);
			return new FileStreamResult(fileStream, contentType);
		}
	}
}
