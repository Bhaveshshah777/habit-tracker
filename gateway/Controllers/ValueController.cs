using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Gateway.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
public class ValueController : ControllerBase
{
    private readonly ITokenBlackListing _redisService;

    public ValueController(ITokenBlackListing redisService)
    {
        _redisService = redisService;
    }

    [HttpGet]
    [Route("/")]  // Keeping the root endpoint
    [AllowAnonymous]
    public IActionResult Root() => Ok("Hello World!");

    [HttpGet]
    [Route("/user-details")]
    public IActionResult GetUserDetails()
    {
        var name = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "N/A";
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "N/A";

        return Ok(new { Name = name, Email = email });
    }

    [HttpGet]
    [Route("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var callbackUrl = "https://localhost:7110/";

        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Redirect(callbackUrl);

        var handler = new JwtSecurityTokenHandler();
        var token = authHeader.Replace("Bearer ", "").Trim();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null)
            return BadRequest("Invalid token.");

        await _redisService.BlacklistTokenAsync(token);
        return Redirect(callbackUrl);
    }

    [HttpGet]
    [Route("/token")]
    public async Task<IActionResult> GetToken()
    {
        string? idToken = await HttpContext.GetTokenAsync("id_token");
        string? refreshToken = await HttpContext.GetTokenAsync("id_token");

        return Ok(new { IdToken = idToken, Refresh_Token = refreshToken });
    }
}
