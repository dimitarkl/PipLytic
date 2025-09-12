namespace server.Data;

public class DatabaseInitializer
{
    public static void EnsureDatabaseExists(string connectionString)
    {
        var csb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var dbName = csb.Database;

        csb.Database = "postgres";

        using var conn = new Npgsql.NpgsqlConnection(csb.ConnectionString);
        conn.Open();

        var cmd = new Npgsql.NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @dbName", conn);
        cmd.Parameters.AddWithValue("dbName", dbName);

        var exists = cmd.ExecuteScalar() != null;

        if (!exists)
        {
            using var createCmd = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn);
            createCmd.ExecuteNonQuery();
        }
    }
}