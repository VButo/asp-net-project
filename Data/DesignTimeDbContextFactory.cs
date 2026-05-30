using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API_tester.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Prefer explicit migration override if set; otherwise use the real app config.
        var connectionString = Environment.GetEnvironmentVariable("EF_MIGRATION_CONN")
            ?? configuration.GetConnectionString("ApiTesterDb")
            ?? throw new InvalidOperationException("Connection string 'ApiTesterDb' was not found.");

        // Specify the server version explicitly so EF tools do not attempt to connect for AutoDetect.
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}
