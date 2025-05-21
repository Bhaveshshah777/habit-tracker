using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserRepo repo;
    private readonly ILogger<UserController> logger;

    public UserController(UserRepo repo, ILogger<UserController> logger)
    {
        this.repo = repo;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<User>> FetchUser()
    {
        string email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
        string name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
        if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required!");

        var fetchedUser = await repo.FetchUserDetails(email);
        if (fetchedUser is null)
        {
            await repo.CreateUser(email, name);
            fetchedUser = await repo.FetchUserDetails(email);
            if (fetchedUser is null) throw new Exception("Error while making new user entry in database");
        }
        return Ok(fetchedUser);
    }

    [HttpGet]
    public IActionResult GiveHello()
    {
        return Ok("Hello from User Controller!");
    }
}