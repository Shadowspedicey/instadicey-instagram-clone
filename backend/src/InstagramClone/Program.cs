using InstagramClone.Authorization;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using InstagramClone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, IsPostOwnerHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program { }