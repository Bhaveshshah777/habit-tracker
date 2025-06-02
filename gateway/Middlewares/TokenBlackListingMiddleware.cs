
using Gateway.Interfaces;

namespace Gateway.Middlewares;

public class TokenBlackListingMiddleware : IMiddleware
{
    private readonly ITokenBlackListing _blacklistService;

    public TokenBlackListingMiddleware(ITokenBlackListing tokenBlackListingservice)
    {
        this._blacklistService = tokenBlackListingservice;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            var isBlacklisted = await _blacklistService.IsTokenBlacklistedAsync(token);
            if (isBlacklisted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is blacklisted");
                return;
            }
        }
        await next(context);
    }
}