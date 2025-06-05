using System.Data;
using Dapper;
using Npgsql;
using UserService.Kafka.Events;
using UserService.Kafka.Interface;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepo
{
    #region Fields
    private readonly string? cs;
    private readonly IKafkaProducer _kafkaProducer;
    #endregion

    #region Constructor
    public UserRepo(IConfiguration config, IKafkaProducer kafkaProducer)
    {
        cs = config["POSTGRES_CONNECTION"];
        if (string.IsNullOrEmpty(cs))
            throw new Exception("User: POSTGRES_CONNECTION is not set.");
        _kafkaProducer = kafkaProducer;
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
        string sql = "INSERT INTO users (name, email) VALUES (@name, @email)";
        int affectedRows = await con.ExecuteAsync(sql, new { name = name, email = email });
        if (affectedRows == 1)
        {
            var userRegisteredEvent = new UserRegisteredEvent
            {
                Name = name,
                Email = email,
                RegisteredAt = DateTime.UtcNow
            };
            await _kafkaProducer.ProduceAsync<UserRegisteredEvent>("user.registered", userRegisteredEvent);
        }

        return;
    }
    #endregion

}