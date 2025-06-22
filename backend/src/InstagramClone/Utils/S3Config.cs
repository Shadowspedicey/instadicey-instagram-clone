namespace InstagramClone.Utils
{
	public class S3Config
	{
		public required string AccessKey { get; set; }
		public required string SecretKey { get; set; }
		public string? ServiceURL { get; set; }
	}
}
