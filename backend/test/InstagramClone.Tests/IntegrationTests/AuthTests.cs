using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InstagramClone.Tests.IntegrationTests
{
	public class AuthTests : IClassFixture<AuthTestsWebApplicationFactory>
	{
		private readonly AuthTestsWebApplicationFactory _fixture;
		public AuthTests(AuthTestsWebApplicationFactory fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public async Task RegisterUser_ValidData_ShouldCreateUser()
		{
			var client = _fixture.CreateClient();
			var newUser = new UserRegisterDTO
			{
				Email = "example1@domain.com",
				Username = "exampleusername1",
				Password = "password",
				RealName = "Example Name"
			};

			var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");
			await client.PostAsync("auth/register", content);

			using var scope = _fixture.Services.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			bool userExists = dbContext.Users.Any(u => u.Email == "example@domain.com");
		}

		[Fact]
		public async Task Login_EmailNotVerified_ShouldReturn401WithEmailNotVerifiedError()
		{
			var client = _fixture.CreateClient();
			using var scope = _fixture.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var newUser = new User
			{
				Email = "example2@domain.com",
				EmailConfirmed = false,
				UserName = "exampleusername2",
				RealName = "Example Name",
				IsVerified = false,
				Followers = [],
				Following = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			await userManager.CreateAsync(newUser, "password");

			var loginInfo = new
			{
				newUser.Email,
				Password = "password"
			};
			var response = await client.PostAsJsonAsync("auth/login", loginInfo);

			var responseCode = response.StatusCode;
			Assert.Equal(HttpStatusCode.Unauthorized, responseCode);
		}

		[Fact]
		public async Task Login_EmailVerified_ShouldReturn200WithTokenInResponse()
		{
			var client = _fixture.CreateClient();
			using var scope = _fixture.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var newUser = new User
			{
				Email = "example3@domain.com",
				EmailConfirmed = false,
				UserName = "exampleusername3",
				RealName = "Example Name",
				IsVerified = false,
				Followers = [],
				Following = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			await userManager.CreateAsync(newUser, "password");
			var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
			await userManager.ConfirmEmailAsync(newUser, token);

			var loginInfo = new
			{
				newUser.Email,
				Password = "password"
			};
			var response = await client.PostAsJsonAsync("auth/login", loginInfo);

			var body = await response.Content.ReadAsStreamAsync();
			var bodyJson = JsonSerializer.Deserialize<JsonNode>(body);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.NotNull(bodyJson?["token"]?.GetValue<string>());
		}

		[Fact]
		public async Task Login_InvalidCredentials_ShouldReturn400()
		{
			var client = _fixture.CreateClient();
			using var scope = _fixture.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var newUser = new User
			{
				Email = "example3@domain.com",
				EmailConfirmed = false,
				UserName = "exampleusername3",
				RealName = "Example Name",
				IsVerified = false,
				Followers = [],
				Following = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			await userManager.CreateAsync(newUser, "password");
			var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
			await userManager.ConfirmEmailAsync(newUser, token);

			var loginInfo = new
			{
				newUser.Email,
				Password = "wrong-password"
			};
			var response = await client.PostAsJsonAsync("auth/login", loginInfo);

			var body = await response.Content.ReadAsStreamAsync();
			var bodyJson = JsonSerializer.Deserialize<JsonNode>(body);
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Equal(bodyJson?["errors"][0]["code"]?.GetValue<string>(), "InvalidCredentials");
		}

		[Fact]
		public async Task ConfirmEmail_ShouldReturn204_WhenValidToken()
		{
			var client = _fixture.CreateClient();
			using var scope = _fixture.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var newUser = new User
			{
				Email = "example4@domain.com",
				EmailConfirmed = false,
				UserName = "exampleusername4",
				RealName = "Example Name",
				IsVerified = false,
				Followers = [],
				Following = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			await userManager.CreateAsync(newUser, "password");
			var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);

			var encodedEmail = WebUtility.UrlEncode(newUser.Email);
			var encodedToken = WebUtility.UrlEncode(token);
			var response = await client.PostAsync($"auth/confirm-email?encodedEmail={encodedEmail}&code={encodedToken}", null);


			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task ConfirmEmail_ShouldReturn400_WhenInvalidToken()
		{
			var client = _fixture.CreateClient();
			using var scope = _fixture.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var newUser = new User
			{
				Email = "example5@domain.com",
				EmailConfirmed = false,
				UserName = "exampleusername5",
				RealName = "Example Name",
				IsVerified = false,
				Followers = [],
				Following = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			await userManager.CreateAsync(newUser, "password");

			var encodedEmail = WebUtility.UrlEncode(newUser.Email);
			var response = await client.PostAsync($"auth/confirm-email?encodedEmail={encodedEmail}&code=InvalidToken", null);


			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task SendEmailChangeRequest_ShouldReturn404_WhenGuest()
		{
			var client = _fixture.CreateClient();
			string? token = await LoginAsGuest(client);

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var emailChangeRequestResponse = await client.PostAsJsonAsync("auth/send-email-change-verification", new {Email = "example@domain.com", Password = "password"});

			Assert.Equal(HttpStatusCode.Forbidden, emailChangeRequestResponse.StatusCode);
		}

		[Fact]
		public async Task ChangePassword_ShouldReturn404_WhenGuest()
		{
			var client = _fixture.CreateClient();
			string? token = await LoginAsGuest(client);

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var emailChangeRequestResponse = await client.PostAsJsonAsync("auth/change-password", new {CurrentPassword = "currentPassword", NewPassword = "newPassword"});
			
			Assert.Equal(HttpStatusCode.Forbidden, emailChangeRequestResponse.StatusCode);
		}

		[Fact]
		public async Task ChangeUsername_ShouldReturn404_WhenGuest()
		{
			var client = _fixture.CreateClient();
			string? token = await LoginAsGuest(client);

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var changeUsernameResponse = await client.PostAsJsonAsync("user/edit", new { Username = "newUsername", RealName = "anything", Bio = "anything" });

			Assert.Equal(HttpStatusCode.Forbidden, changeUsernameResponse.StatusCode);
		}

		[Fact]
		public async Task EditUserData_ShouldReturn204_WhenGuest()
		{
			var client = _fixture.CreateClient();
			string? token = await LoginAsGuest(client);

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var changeUsernameResponse = await client.PostAsJsonAsync("user/edit", new { Username = "guest", RealName = "anything", Bio = "anything" });

			Assert.Equal(HttpStatusCode.NoContent, changeUsernameResponse.StatusCode);
		}

		[Fact]
		public async Task ChatHub_ShouldReturn404_WhenGuest()
		{
			var client = _fixture.CreateClient();
			var connection = new HubConnectionBuilder()
				.WithUrl("http://localhost/chat-hub", options =>
				{
					options.HttpMessageHandlerFactory = _ => _fixture.Server.CreateHandler();
					options.AccessTokenProvider = async () => await LoginAsGuest(client);
				})
				.WithAutomaticReconnect()
				.Build();


			try
			{
				await connection.StartAsync();
			}
			catch (HttpRequestException ex)
			{
				Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
			}
		}

		private static async Task<string?> LoginAsGuest(HttpClient client)
		{
			var loginResponse = await client.PostAsJsonAsync("auth/login", new { email = "guest@instadicey.com", password = "" });
			loginResponse.EnsureSuccessStatusCode();
			var loginResponseJson = JsonSerializer.Deserialize<JsonNode>(await loginResponse.Content.ReadAsStreamAsync());
			return loginResponseJson?["token"]?.ToString();
		}
	}
	
	public class AuthTestsWebApplicationFactory : WebApplicationFactory<Program>
	{
		private SqliteConnection _connection;
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			builder.ConfigureAppConfiguration((context, config) =>
			{
				config.AddUserSecrets<AuthTests>();
			});
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<DbContextOptions<AppDbContext>>();
				services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))!);
				services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
			});
		}

		protected override IHost CreateHost(IHostBuilder builder)
		{
			var host = base.CreateHost(builder);

			using var scope = host.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();

			return host;
		}
	}
}
