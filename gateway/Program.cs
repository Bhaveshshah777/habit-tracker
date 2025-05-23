#region Usings
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
#endregion

#region Configurations
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "https://+:443");

var jwtSetting = builder.Configuration.GetSection("Authentication:Jwt");
var googleSetting = builder.Configuration.GetSection("Authentication:Google");

var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

builder.Services.AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy")
    );

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartScheme";
    options.DefaultChallengeScheme = "Google";
})
.AddPolicyScheme("SmartScheme", "Smart Scheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // If request has a Bearer token, use JWT Bearer scheme
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.StartsWith("Bearer ") == true ? "Bearer" : CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddCookie()
.AddOpenIdConnect("Google", options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.Authority = googleSetting["Authority"];
    options.CallbackPath = googleSetting["CallbackPath"];
    options.SaveTokens = true;
    options.ResponseType = googleSetting["ResponseType"] ?? "id_token token";
    options.GetClaimsFromUserInfoEndpoint = true;


    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
})
.AddJwtBearer("Bearer", jwtOptions =>
{
    jwtOptions.Authority = jwtSetting["Authority"];
    jwtOptions.Audience = googleClientId;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true
    };
});

var requireAuthPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(requireAuthPolicy);

builder.Services.AddAuthorization();
#endregion

#region App
var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
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

app.MapReverseProxy();
app.MapGet("/", [AllowAnonymous] () => "Hello World!");
app.MapGet("/test-auth", () => "You're authenticated");
app.MapGet("/debug-auth", (HttpContext ctx) =>
{
    var token = ctx.Request.Headers["Authorization"].FirstOrDefault();
    var authType = ctx.User?.Identity?.AuthenticationType ?? "None";
    var isAuth = ctx.User?.Identity?.IsAuthenticated ?? false;
    var name = ctx.User?.Identity?.Name ?? "Anonymous";

    return Results.Ok(new
    {
        TokenHeader = token,
        AuthenticationType = authType,
        IsAuthenticated = isAuth,
        Name = name
    });
});
app.MapGet("/user-details", (ClaimsPrincipal user) =>
{
    var name = user.FindFirst(ClaimTypes.GivenName)?.Value ?? "N/A";
    var email = user.FindFirst(ClaimTypes.Email)?.Value ?? "N/A";

    return new
    {
        Name = name,
        Email = email
    };
});
app.MapGet("/logout", [AllowAnonymous] async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    // This URL logs the user out from Google
    var callbackUrl = "https://localhost:7110/";
    var googleLogoutUrl =
        $"https://accounts.google.com/Logout?continue=https://appengine.google.com/_ah/logout?continue={Uri.EscapeDataString(callbackUrl)}";

    return Results.Redirect(googleLogoutUrl);
});
app.MapGet("/token", async (HttpContext context) =>
{
    var idToken = await context.GetTokenAsync("id_token");
    return Results.Ok(new { IdToken = idToken });
});
app.Run();
#endregion