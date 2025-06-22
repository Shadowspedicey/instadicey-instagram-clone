using InstagramClone.Authorization;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Hubs;
using InstagramClone.Interfaces;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["FrontendOrigin"] = Helpers.GetHostFromURL(builder.Configuration["Frontend"] ?? throw new ArgumentNullException("FrontendOrigin", "Frontend origin has to be set in appsettings.json for CORS to be configured."));

var signingKeys = builder.Configuration.GetRequiredSection("Authentication:Schemes:Bearer:SigningKeys").GetChildren().ToList().Select(sk => new SymmetricSecurityKey(Convert.FromBase64String(sk?["Value"])));
//Console.WriteLine(JsonSerializer.Serialize(builder.Configuration.GetRequiredSection("Authentication:Schemes:Bearer:SigningKeys")));
var jwtValidationParameters = new TokenValidationParameters
{
	ValidateIssuer = false,
	ValidateAudience = false,
	ValidateIssuerSigningKey = true,
	ClockSkew = TimeSpan.Zero,
	IssuerSigningKeys = signingKeys
};
builder.Services.AddSingleton(jwtValidationParameters);

builder.Services.AddProblemDetails();
builder.Services.AddControllers()
	.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => 
	options
		.UseSqlServer(builder.Configuration.GetConnectionString("Main"))
		.UseLazyLoadingProxies());
builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer(options =>
	{
		options.MapInboundClaims = false;
		options.TokenValidationParameters = jwtValidationParameters;
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var accessToken = context.Request.Query["access_token"];
				var path = context.HttpContext.Request.Path;
				if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat-hub"))
					context.Token = accessToken;
				return Task.CompletedTask;
			}
		};
	});
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("CanDeletePost", policyBuilder =>
	{
		policyBuilder.AddRequirements(new IsPostOwnerRequirement());
	});
	options.AddPolicy("CanDeleteComment", policyBuilder =>
	{
		policyBuilder.AddRequirements(new IsCommentOrPostOwnerRequirement());
	});
	options.AddPolicy("CanAccessRoomMessages", policyBuilder =>
	{
		policyBuilder.AddRequirements(new IsInChatRoomRequirement());
	});
	options.AddPolicy("IsNotGuest", policyBuilder =>
	{
		policyBuilder.AddRequirements(new IsNotGuestRequirement());
});
});
builder.Services.AddIdentityCore<User>(options =>
{
	options.ClaimsIdentity.UserIdClaimType = "sub";
	options.ClaimsIdentity.UserNameClaimType = "username";
	options.ClaimsIdentity.EmailClaimType = "email";

	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
	options.Password.RequireDigit = false;
	options.Password.RequiredUniqueChars = 0;
	options.Password.RequireUppercase = false;

	options.User.RequireUniqueEmail = true;

	options.SignIn.RequireConfirmedEmail = true;
})
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();
builder.Services.AddCors(corsBuilder =>
{
	string frontendOrigin = builder.Configuration["FrontendOrigin"]!;
	corsBuilder.AddPolicy("Default", policy =>
	{
		policy
			.WithOrigins(frontendOrigin)
			.WithMethods("GET", "POST")
			.WithHeaders("Content-Type", "Authorization");
	});
	corsBuilder.AddPolicy("Hub", policy => policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPostsService, PostsService>();
if (bool.Parse(builder.Configuration["UseS3Cloud"]!) == true)
{
	builder.Services.Configure<S3Config>(builder.Configuration.GetSection("S3"));
	builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<S3Config>>().Value);
	builder.Services.AddSingleton<IFileService, S3FileService>();
}
else
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddSingleton<UserConnectionManager>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IUserIdProvider, UserIDProvider>();
builder.Services.AddHostedService<GuestBackgroundService>();

// Authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, IsPostOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsCommentsPostOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsCommentOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsInChatRoomHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsNotGuestHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseHealthChecks("/ping");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AuthHub>("/email-verification-hub").RequireCors("Hub");
app.MapHub<ChatHub>("/chat-hub").RequireCors("Hub").RequireAuthorization();


app.Run();

public partial class Program { }