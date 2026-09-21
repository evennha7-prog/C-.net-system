using System;
using System.Text;
using Npgsql;

namespace assignment_code.Services.Database
{
    public static class DbConnectionHelper
    {
        public static string GetConnectionString()
        {
            string host = EnvLoader.Get("DB_HOST", "localhost");
            int port = EnvLoader.GetInt("DB_PORT", 5432);
            string database = EnvLoader.Get("DB_DATABASE", "postgres");
            string username = EnvLoader.Get("DB_USERNAME", "postgres");
            string password = EnvLoader.Get("DB_PASSWORD", "");

            var sb = new StringBuilder();
            sb.Append($"Host={host};");
            sb.Append($"Port={port};");
            sb.Append($"Database={database};");
            sb.Append($"Username={username};");
            sb.Append($"Password={password};");
            
            // For cloud-hosted databases (Aiven, Neon, AWS RDS, Supabase)
            sb.Append("SSL Mode=Require;");
            sb.Append("Trust Server Certificate=true;");
            sb.Append("Timeout=4;");
            sb.Append("Command Timeout=10;");

            return sb.ToString();
        }

        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(GetConnectionString());
        }

        public static bool TestConnection(out string message, out long elapsedMs)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            message = string.Empty;
            elapsedMs = 0;

            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT version();", conn))
                    {
                        var version = cmd.ExecuteScalar()?.ToString() ?? "PostgreSQL";
                        sw.Stop();
                        elapsedMs = sw.ElapsedMilliseconds;
                        message = $"Connected successfully to {conn.Host}:{conn.Port} ({version}) in {elapsedMs}ms.";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                message = $"Connection failed: {ex.Message}";
                return false;
            }
        }
    }
}
