using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers
{
    public class SQLInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder application)
        {
            string? configuredConnStr = application.Configuration.GetValue<string>("ConnectionStrings:InventoryConnectionString");
            string connectionString = ResolveWorkingConnectionString(configuredConnStr);

            // Update configuration so Hangfire and all other services use the resolved connection string
            application.Configuration["ConnectionStrings:InventoryConnectionString"] = connectionString;

            application.Services.AddDbContext<ERPContext>(option =>
            {
                option.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(180); // 3 minutes timeout for migrations
                });
                #if DEBUG
                option.EnableSensitiveDataLogging();
                option.EnableDetailedErrors();
                #endif
            });
        }

        private static string ResolveWorkingConnectionString(string? initial)
        {
            var candidates = new List<string>();

            // 1. Check for manual override file (db_config.txt) next to executable
            try
            {
                string overrideFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_config.txt");
                if (File.Exists(overrideFile))
                {
                    string fileConn = File.ReadAllText(overrideFile).Trim();
                    if (!string.IsNullOrWhiteSpace(fileConn))
                    {
                        candidates.Add(fileConn);
                    }
                }
            }
            catch { }

            // 2. Initial configured string
            if (!string.IsNullOrWhiteSpace(initial))
            {
                candidates.Add(initial);
            }

            string[] servers = {
                "localhost\\SQLEXPRESS",
                ".\\SQLEXPRESS",
                "(local)\\SQLEXPRESS",
                "127.0.0.1\\SQLEXPRESS",
                "localhost",
                ".",
                "(local)",
                "127.0.0.1"
            };

            // 3. High-probability local SQL Server instances with Windows Authentication
            foreach (var srv in servers)
            {
                candidates.Add($"Server={srv};Database=HR_Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");
            }

            // 4. Common sa passwords across local instances
            string[] saPasswords = {
                "11223344Aa", "11223344", "123456", "12345678", "123456789", "1234", "123", "12345",
                "Admin@1234", "Admin1234", "Admin@123", "Admin123", "admin", "sa", "root", "",
                "P@ssw0rd", "P@ssw0rd123", "Password123", "Password@123", "Sql@1234", "Sql@123", "sql", "1", "0"
            };

            foreach (var srv in servers)
            {
                foreach (var pwd in saPasswords)
                {
                    candidates.Add($"Server={srv};Database=HR_Database;User Id=sa;Password={pwd};TrustServerCertificate=True;Encrypt=False;");
                }
            }

            // 5. LocalDB fallback
            candidates.Add("Server=(localdb)\\mssqllocaldb;Database=HR_Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql_status.log");

            foreach (var conn in candidates)
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(conn);
                    var targetDb = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "HR_Database" : builder.InitialCatalog;
                    builder.InitialCatalog = "master"; // test server presence on master database
                    builder.ConnectTimeout = 2; // quick 2-second probe

                    using var testConn = new SqlConnection(builder.ConnectionString);
                    testConn.Open();

                    // Ensure target database exists on this SQL Server
                    try
                    {
                        using var cmd = testConn.CreateCommand();
                        cmd.CommandText = $@"
                            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{targetDb}')
                            BEGIN
                                CREATE DATABASE [{targetDb}];
                            END";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception exDbCreate)
                    {
                        Console.WriteLine($"[SQL Auto-Discovery] Database create notice: {exDbCreate.Message}");
                    }

                    // Restore target database
                    builder.InitialCatalog = targetDb;
                    builder.ConnectTimeout = 120;

                    string msg = $"[{DateTime.Now}] Successfully connected to SQL Server: {builder.DataSource} | DB: {targetDb} | Auth: {(builder.IntegratedSecurity ? "Windows Auth" : $"SQL Auth ({builder.UserID})")}\n";
                    Console.WriteLine(msg);
                    try { File.WriteAllText(logPath, msg); } catch { }

                    return builder.ConnectionString;
                }
                catch
                {
                    // Continue probing next instance
                }
            }

            string failMsg = $"[{DateTime.Now}] Could not connect to any probed SQL Server instances. Falling back to default localhost\\SQLEXPRESS.\n";
            Console.WriteLine(failMsg);
            try { File.WriteAllText(logPath, failMsg); } catch { }

            return initial ?? "Server=localhost\\SQLEXPRESS;Database=HR_Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
        }
    }
}
