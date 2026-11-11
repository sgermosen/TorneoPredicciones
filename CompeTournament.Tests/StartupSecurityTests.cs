using System;
using System.IO;
using CompeTournament.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CompeTournament.Tests
{
    public class StartupSecurityTests
    {
        [Fact]
        public void Production_RejectsExampleSigningKey()
        {
            using var factory = new ProductionFactory("local-dev-only-signing-key-change-me-0123456789-0123456789", "Demo-Clave-2026");

            var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            Assert.Contains("clave de ejemplo", exception.ToString());
        }

        [Fact]
        public void Production_RequiresSeedPassword()
        {
            using var factory = new ProductionFactory("clave-propia-de-produccion-con-longitud-suficiente-9f8e7d6c5b4a", null);

            var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            Assert.Contains("Seed:DefaultPassword", exception.ToString());
        }

        private class ProductionFactory : WebApplicationFactory<Program>
        {
            private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"ct-prod-{Guid.NewGuid():N}.db");
            private readonly string _signingKey;
            private readonly string? _seedPassword;

            public ProductionFactory(string signingKey, string? seedPassword)
            {
                _signingKey = signingKey;
                _seedPassword = seedPassword;
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Production");
                builder.UseSetting("Database:Provider", "Sqlite");
                builder.UseSetting("ConnectionStrings:SqliteCnn", $"Data Source={_dbPath}");
                builder.UseSetting("Tokens:Key", _signingKey);
                builder.UseSetting("Scheduler:Enabled", "false");
                if (_seedPassword != null)
                {
                    builder.UseSetting("Seed:DefaultPassword", _seedPassword);
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                SqliteConnection.ClearAllPools();
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
        }
    }
}
