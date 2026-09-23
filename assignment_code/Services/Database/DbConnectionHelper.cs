using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Npgsql;

namespace assignment_code.Services.Database
{
    public enum DatabaseProvider
    {
        SqlServer,
        PostgreSql
    }

    public static class DbConnectionHelper
    {
        public static DatabaseProvider CurrentProvider
        {
            get
            {
                string connType = EnvLoader.Get("DB_CONNECTION", "sqlserver").ToLowerInvariant().Trim();
                if (connType.Contains("pgsql") || connType.Contains("postgres"))
                {
                    return DatabaseProvider.PostgreSql;
                }

                // Default is SQL Server
                return DatabaseProvider.SqlServer;
            }
        }

        public static bool IsSqlServer => CurrentProvider == DatabaseProvider.SqlServer;
        public static bool IsPostgreSql => CurrentProvider == DatabaseProvider.PostgreSql;

        public static string ProviderDisplayName => IsSqlServer ? "Microsoft SQL Server" : "PostgreSQL";

        public static string ServerDisplayName
        {
            get
            {
                if (IsSqlServer)
                {
                    return EnvLoader.Get("DB_HOST", @".\SQLEXPRESS");
                }
                string host = EnvLoader.Get("DB_HOST", "localhost");
                int port = EnvLoader.GetInt("DB_PORT", 5432);
                return $"{host}:{port}";
            }
        }

        public static string GetConnectionString()
        {
            if (IsSqlServer)
            {
                string server = EnvLoader.Get("DB_HOST", @".\SQLEXPRESS");
                string database = EnvLoader.Get("DB_DATABASE", "mart_pccfpi");
                bool trusted = EnvLoader.GetBool("DB_TRUSTED_CONNECTION", true);
                string username = EnvLoader.Get("DB_USERNAME", "");
                string password = EnvLoader.Get("DB_PASSWORD", "");

                var sb = new StringBuilder();
                sb.Append($"Server={server};");
                sb.Append($"Database={database};");

                if (trusted || string.IsNullOrWhiteSpace(username))
                {
                    sb.Append("Integrated Security=True;");
                }
                else
                {
                    sb.Append($"User Id={username};");
                    sb.Append($"Password={password};");
                }

                sb.Append("TrustServerCertificate=True;");
                sb.Append("Connect Timeout=8;");
                return sb.ToString();
            }
            else
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
                sb.Append("SSL Mode=Require;");
                sb.Append("Trust Server Certificate=true;");
                sb.Append("Timeout=6;");
                sb.Append("Command Timeout=15;");

                return sb.ToString();
            }
        }

        public static DbConnection CreateConnection()
        {
            if (IsSqlServer)
            {
                return new SqlConnection(GetConnectionString());
            }
            return new NpgsqlConnection(GetConnectionString());
        }

        public static DbCommand CreateCommand(DbConnection conn, string sql, DbTransaction tx = null)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (tx != null) cmd.Transaction = tx;
            return cmd;
        }

        public static DbParameter AddParam(this DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
            return param;
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

                    if (IsSqlServer)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT @@VERSION;";
                            var verObj = cmd.ExecuteScalar()?.ToString() ?? "Microsoft SQL Server";
                            string firstLine = verObj.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            sw.Stop();
                            elapsedMs = sw.ElapsedMilliseconds;
                            message = $"Connected successfully to SQL Server [{conn.Database}] on [{conn.DataSource}] in {elapsedMs}ms.\nVersion: {firstLine}";
                            return true;
                        }
                    }
                    else
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT version();";
                            var version = cmd.ExecuteScalar()?.ToString() ?? "PostgreSQL";
                            sw.Stop();
                            elapsedMs = sw.ElapsedMilliseconds;
                            message = $"Connected successfully to PostgreSQL [{conn.Database}] at {conn.DataSource} ({version}) in {elapsedMs}ms.";
                            return true;
                        }
                    }
                }
            }
            catch (SqlException sqlex)
            {
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                message = $"SQL Server Error [{sqlex.Number}]: {sqlex.Message}\n" +
                          $"Server: {EnvLoader.Get("DB_HOST")}, Database: {EnvLoader.Get("DB_DATABASE")}";
                return false;
            }
            catch (PostgresException pgex)
            {
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                if (pgex.SqlState == "28P01" || pgex.MessageText.ToLowerInvariant().Contains("password authentication failed"))
                {
                    message = "Password authentication failed (28P01). The database password in your .env does not match your database password.";
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
                message = $"Connection failed: {ex.Message}";
                return false;
            }
        }
    }
}
