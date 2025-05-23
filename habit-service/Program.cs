using System.Data;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));
builder.Services.AddControllers();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Welcome to habit service.");
app.Run();
