using InstagramClone.Interfaces;

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
	}
}
