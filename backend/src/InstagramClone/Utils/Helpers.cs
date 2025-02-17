using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Cryptography;

namespace InstagramClone.Utils
{
	public static class Helpers
	{
		private static readonly IConfiguration config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<Program>()
			.Build();

		public static string GetHostFromURL(string url)
		{
			Uri uri = new Uri(url);
			return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
		}

		public static class Files
		{
			public static async Task<string> SavePost(
				IFileService fileService,
				IFormFile post,
				string userID,
				string postID,
				CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				return await fileService.SaveFile(post, Path.Combine(userID, "posts"), $"{postID}{Path.GetExtension(post.FileName)}", cancellationToken);
			}

			public static string GetMimeTypeForFileExtension(string filePath)
			{
				const string defaultContentType = "application/octet-stream";

				var provider = new FileExtensionContentTypeProvider();

				if (!provider.TryGetContentType(filePath, out string? contentType))
					contentType = defaultContentType;

				return contentType;
			}
		}

		public static class Encryption
		{
			private static readonly byte[] Key = Convert.FromBase64String(config["AES:Key"]);
			private static readonly byte[] IV = Convert.FromBase64String(config["AES:IV"]);

			public static string Encrypt(string value)
			{
				Console.WriteLine("Encrypting " + value);
				using (Aes aesAlg = Aes.Create())
				{
					aesAlg.Key = Key;
					aesAlg.IV = IV;

					var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
					using (var msEncrypt = new MemoryStream())
					{
						using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
							using (var swEncrypt = new StreamWriter(csEncrypt))
								swEncrypt.Write(value);

						return ToUrlSafeBase64(Convert.ToBase64String(msEncrypt.ToArray()));
					}
				}
			}

			public static string Decrypt(string encryptedValue)
			{
				encryptedValue = FromUrlSafeBase64(encryptedValue);
				if (!IsValidAesText(encryptedValue))
					throw new ArgumentException("InvalidInput");
				using (Aes aesAlg = Aes.Create())
				{
					aesAlg.Key = Key;
					aesAlg.IV = IV;

					var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

					using (var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedValue)))
					{
						using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
							using (var srDecrypt = new StreamReader(csDecrypt))
								return srDecrypt.ReadToEnd();
					}
				}
			}

			private static string ToUrlSafeBase64(string base64)
			{
				return base64.Replace("+", "-")
							 .Replace("/", "_")
							 .Replace("=", ""); // Remove padding
			}

			private static string FromUrlSafeBase64(string urlSafeBase64)
			{
				string base64 = urlSafeBase64.Replace("-", "+")
											 .Replace("_", "/");
				// Add padding if necessary
				switch (base64.Length % 4)
				{
					case 2: base64 += "=="; break;
					case 3: base64 += "="; break;
				}
				return base64;
			}

			private static bool IsValidAesText(string encryptedString)
			{
				byte[] cipherBytes;
				try
				{
					cipherBytes = Convert.FromBase64String(encryptedString);
				}
				catch
				{
					return false;
				}

				if (cipherBytes.Length % 16 != 0)
					return false;
				return true;
			}
		}
	}
}
