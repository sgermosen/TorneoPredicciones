using System;
using System.IO;
using CompeTournament.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CompeTournament.Tests
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"ct-test-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Database:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:SqliteCnn", $"Data Source={_dbPath}");
            builder.UseSetting("Tokens:Key", "integration-test-signing-key-should-be-long-enough-0123456789");
            builder.UseSetting("Seed:DefaultPassword", "Torneo2026");
            builder.UseSetting("Scheduler:Enabled", "false");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
    }
}
