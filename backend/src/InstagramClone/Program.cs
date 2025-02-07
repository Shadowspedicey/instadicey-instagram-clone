using InstagramClone.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("", () => "Hey").RequireAuthorization();

app.Run();
