using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using FluentResults;
using InstagramClone.Interfaces;
using InstagramClone.Utils;

namespace InstagramClone.Services
{
	public class S3FileService : IFileService
	{
		private readonly AmazonS3Client _client;
		public S3FileService(S3Config _config)
		{
			var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
			var config = new AmazonS3Config
			{
				RegionEndpoint = RegionEndpoint.EUCentral1,
				ServiceURL = _config.ServiceURL,
				ForcePathStyle = true,
			};
			_client = new AmazonS3Client(credentials, config);
		}
		public async Task<Result<MemoryStream>> GetFile(string filePath)
		{
			var request = new GetObjectRequest
			{
				BucketName = "app-data",
				Key = filePath,
			};

			MemoryStream memoryStream = new();
			try
			{
				var response = await _client.GetObjectAsync(request);
				response.ResponseStream.CopyTo(memoryStream);
			}
			catch (AmazonS3Exception e)
			{
				if (e.Message == "Object not found")
					return Result.Fail(new CodedError(ErrorCode.NotFound, "File was not found."));
			}
			memoryStream.Position = 0;
			return Result.Ok(memoryStream);
			
		}
		public async Task<string> SaveFile(IFormFile file, string path, string fileName, CancellationToken cancellationToken)
		{
			var _path = Path.Combine(path, fileName).Replace("\\", "/");
			using var fileStream = file.OpenReadStream();
			var request = new PutObjectRequest()
			{
				BucketName = "app-data",
				Key = _path,
				InputStream = fileStream,
				ContentType = file.ContentType

			};
			await _client.PutObjectAsync(request);
			return _path;
		}

		public void DeleteFile(string filePath)
		{
			var request = new DeleteObjectRequest
			{
				BucketName = "app-data",
				Key = filePath
			};
			_client.DeleteObjectAsync(request);
		}

		public void DeleteFolder(string folderPath)
		{
			var listRequest = new ListObjectsRequest
			{
				BucketName = "app-data",
				Prefix = folderPath,
			};
			var deleteRequest = new DeleteObjectsRequest
			{
				BucketName = "app-data"
			};

			var response = _client.ListObjectsAsync(listRequest).Result;
			if (response.S3Objects == null)
				return;
			foreach (var obj in response.S3Objects)
				deleteRequest.AddKey(obj.Key);
			_client.DeleteObjectsAsync(deleteRequest);
		}
	}
}
