using InstagramClone.Interfaces;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace InstagramClone.Utils
{
	public static class Helpers
	{
		public static ObjectResult ProblemWithErrors(this ControllerBase controllerBase, object errors, int statusCode, string? detail = null)
		{
			return controllerBase.Problem(
				statusCode: statusCode,
				extensions: new Dictionary<string, object?> { { "errors", errors } },
				detail: detail
			);
		}

		public static class Files
		{
			public static async Task<string> SavePost(IFileService fileService, IFormFile post, string userID, string postID) => await fileService.SaveFile(post, Path.Combine(userID, "posts"), $"{postID}{Path.GetExtension(post.FileName)}");

			public static string GetMimeTypeForFileExtension(string filePath)
			{
				const string defaultContentType = "application/octet-stream";

				var provider = new FileExtensionContentTypeProvider();

				if (!provider.TryGetContentType(filePath, out string? contentType))
					contentType = defaultContentType;

				return contentType;
			}
		}
	}
}
