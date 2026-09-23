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
            catch (PostgresException pgex)
            {
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                if (pgex.SqlState == "28P01" || pgex.MessageText.ToLowerInvariant().Contains("password authentication failed"))
                {
                    message = "Password authentication failed (28P01). The database password in your .env does not match your Supabase database password.\n" +
                              "Please reset your password in Supabase Dashboard (Project Settings > Database > Database password) and update DB_PASSWORD in .env.";
                }
                else
                {
                    message = $"PostgreSQL Error [{pgex.SqlState}]: {pgex.MessageText}";
                }
                return false;
            }
            catch (Exception ex)
            {
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                if (ex.Message.Contains("28P01") || ex.Message.ToLowerInvariant().Contains("password authentication failed"))
                {
                    message = "Password authentication failed (28P01). The database password in your .env does not match your Supabase database password.\n" +
                              "Please reset your password in Supabase Dashboard (Project Settings > Database > Database password) and update DB_PASSWORD in .env.";
                }
                else
                {
                    message = $"Connection failed: {ex.Message}";
                }
                return false;
            }
        }
    }
}
