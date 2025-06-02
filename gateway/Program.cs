using Gateway.Middlewares;
using Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "https://+:443");
builder.Services.AddAuthenticationConfiguration(builder.Configuration); // All logic related to authentication 
builder.Services.AddRedisConfiguration(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy")
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterServices();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<TokenBlackListingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

//Authenticate before transfering requests to other apis
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (path.StartsWithSegments("/user") || path.StartsWithSegments("/habit") || path.StartsWithSegments("/tracking"))
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }

    await next();
});

app.MapControllers();
app.MapReverseProxy();

app.Run();
