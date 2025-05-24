using System.Data;
using Dapper;
using Npgsql;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepo
{
    #region Fields
    private readonly string? cs;
    #endregion

    #region Constructor
    public UserRepo(IConfiguration config)
    {
        cs = config["POSTGRES_CONNECTION"];
        if (string.IsNullOrEmpty(cs))
            throw new Exception("User: POSTGRES_CONNECTION is not set.");
    }
    #endregion

    #region Methods
    /// <summary>
    /// Fetch the users details from the database
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User?> FetchUserDetails(string email)
    {
        await using var con = new NpgsqlConnection(cs);
        var query = "SELECT id, name, email FROM users WHERE email = @Email";
        return await con.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
    }

    public async Task CreateUser(string email, string name)
    {
        await using var con = new NpgsqlConnection(cs);
        var sql = "INSERT INTO users (name, email) VALUES (@name, @email)";
        await con.ExecuteScalarAsync(sql, new { name = name, email = email });
        return;
    }
    #endregion

}