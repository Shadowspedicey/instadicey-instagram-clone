using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Services;
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

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Main")));
builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer(options =>
	{
		options.MapInboundClaims = false;
		options.TokenValidationParameters = jwtValidationParameters;
	});
builder.Services.AddAuthorization();
builder.Services.AddIdentityCore<User>(options =>
{
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
	options.Password.RequireDigit = false;
	options.Password.RequiredUniqueChars = 0;
	options.Password.RequireUppercase = false;

	options.User.RequireUniqueEmail = true;
})
	.AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
