using FluentResults;
using InstagramClone.Interfaces;
using InstagramClone.Utils;

namespace InstagramClone.Services
{
	public class FileService(IConfiguration configuration) : IFileService
	{
		private readonly IConfiguration _configuration = configuration;
		public async Task<string> SaveFile(IFormFile file, string path, string fileName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string dataFolder = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDataFolderName"]);
			string uploadFolder = Path.Combine(dataFolder, path);
			if (!Directory.Exists(uploadFolder))
				Directory.CreateDirectory(uploadFolder);

			string filePath = Path.Combine(uploadFolder, fileName);

			using var stream = new FileStream(filePath, FileMode.CreateNew);
			await file.CopyToAsync(stream, cancellationToken);

			return Path.Combine(path, fileName);
		}

		public async Task<Result<MemoryStream>> GetFile(string filePath)
		{
			var filePathParts = filePath.Split(['\\', '/']);
			string path = Path.GetFullPath($"{_configuration["AppDataFolderName"]}\\{string.Join("\\", filePathParts)}");

			if (!File.Exists(path))
				return Result.Fail("NotFound");

			string fileName = filePathParts.Last();
			var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(path));
			return Result.Ok(memoryStream);
		}

		public void DeleteFile(string path)
		{
			string fullPath = Path.GetFullPath($"{_configuration["AppDataFolderName"]}\\{path}");

			if (File.Exists(fullPath))
				File.Delete(fullPath);
		}
	}
}
