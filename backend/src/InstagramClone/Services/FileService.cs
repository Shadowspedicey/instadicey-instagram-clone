using FluentResults;
using InstagramClone.Interfaces;
using InstagramClone.Utils;

namespace InstagramClone.Services
{
	public class FileService(IConfiguration configuration) : IFileService
	{
		private readonly IConfiguration _configuration = configuration;
		public async Task<string> SaveFile(IFormFile file, string path, string fileName)
		{
			string dataFolder = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDataFolderName"]);
			string uploadFolder = Path.Combine(dataFolder, path);
			if (!Directory.Exists(uploadFolder))
				Directory.CreateDirectory(uploadFolder);

			string filePath = Path.Combine(uploadFolder, fileName);

			using var stream = new FileStream(filePath, FileMode.CreateNew);
			await file.CopyToAsync(stream);

			return Path.Combine(path, fileName);
		}

		public async Task<Result<(MemoryStream, string)>> GetFile(string encryptedFilePath)
		{
			string decryptedFilePath;
			try
			{
				decryptedFilePath = Helpers.Encryption.Decrypt(encryptedFilePath);
			}
			catch (ArgumentException)
			{
				return Result.Fail("Invalid input.");
			}

			var decryptedFilePathParts = decryptedFilePath.Split(['\\', '/']);
			string path = Path.GetFullPath($"{_configuration["AppDataFolderName"]}\\{string.Join("\\", decryptedFilePathParts)}");

			if (!File.Exists(path))
				return Result.Fail("NotFound");

			string fileName = decryptedFilePathParts.Last();
			var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(path));
			return Result.Ok((memoryStream, fileName));
		}
	}
}
