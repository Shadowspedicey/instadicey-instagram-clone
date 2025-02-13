using FluentResults;
using System.IO;

namespace InstagramClone.Interfaces
{
	public interface IFileService
	{
		public Task<string> SaveFile(IFormFile file, string path, string fileName, CancellationToken cancellationToken);
		public Task<Result<MemoryStream>> GetFile(string filePath);
		public void DeleteFile(string filePath);
	}
}
