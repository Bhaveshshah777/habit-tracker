using Gateway.Interfaces;
using Gateway.Middlewares;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;

namespace Gateway.Services;

public static class ServiceRegistration
{
    /// <summary>
    /// This function conigures all the services for dependancy injections
    /// </summary>    
    /// <returns></returns>
    public static IServiceCollection RegisterServices(this IServiceCollection service)
    {
        //Singleton services
        service.AddSingleton<ITokenBlackListing, TokenBlackListingService>();

        //Trasient services
        service.AddTransient<TokenBlackListingMiddleware>();

        return service;
    }

    /// <summary>
    /// Adding Authentication configurations
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection service, IConfiguration configuration)
    {
        var jwtSetting = configuration.GetSection("Authentication:Jwt");
        var googleSetting = configuration.GetSection("Authentication:Google");

        var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
        var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

        service.AddAuthentication(options =>
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

        service.AddAuthorization();
        var requireAuthPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        service.AddAuthorizationBuilder().SetFallbackPolicy(requireAuthPolicy);
        return service;
    }

    /// <summary>
    /// Adding Redis related configuration
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConnection = configuration["REDIS_CONNECTION"];
        if (string.IsNullOrEmpty(redisConnection))
            throw new InvalidOperationException("REDIS_CONNECTION environment variable is not set.");

        service.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection));

        return service;
    }
}
