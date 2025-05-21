using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
if (string.IsNullOrEmpty(connectionString))
    throw new Exception("Connection string missing.");

NpgsqlConnection? conn = null;

try
{
    conn = new NpgsqlConnection(connectionString);
    conn.Open();

    foreach (var file in Directory.GetFiles("Migrations", "*.sql"))
    {
        var sql = File.ReadAllText(file);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
        Console.WriteLine($"✅ Executed {Path.GetFileName(file)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Migration failed: {ex.Message}");
}
finally
{
    if (conn is { State: System.Data.ConnectionState.Open })
    {
        conn.Close();
        Console.WriteLine("🔌 Connection closed.");
    }
}
