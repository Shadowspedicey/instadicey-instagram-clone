namespace InstagramClone.DTOs.Profile
{
	public class UserMinimalProfileDTO(string username, string realName, string profilePic, string downloadEndpoint)
	{
		public string Username { get; set; } = username;
		public string ProfilePic { get; set; } = $"{downloadEndpoint}{profilePic}";
		public string RealName { get; set; } = realName;
	}
}
