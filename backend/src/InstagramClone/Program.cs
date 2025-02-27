using InstagramClone.Authorization;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Hubs;
using InstagramClone.Interfaces;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["FrontendOrigin"] = Helpers.GetHostFromURL(builder.Configuration["Frontend"] ?? throw new ArgumentNullException("FrontendOrigin", "Frontend origin has to be set in appsettings.json for CORS to be configured."));

var signingKeys = builder.Configuration.GetRequiredSection("Authentication:Schemes:Bearer:SigningKeys").GetChildren().ToList();
var jwtValidationParameters = new TokenValidationParameters
{
	ValidateIssuer = false,
	ValidateAudience = false,
	ValidateIssuerSigningKey = true,
	IssuerSigningKeys = new List<SecurityKey>
	{
		new SymmetricSecurityKey(Convert.FromBase64String(signingKeys[0]["Value"])),
		new SymmetricSecurityKey(Convert.FromBase64String(signingKeys[1]["Value"]))
	}
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
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddSingleton<UserConnectionManager>();

// Authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, IsPostOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsCommentsPostOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsCommentOwnerHandler>();

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


app.Run();

public partial class Program { }