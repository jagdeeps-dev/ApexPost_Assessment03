using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ApexPost.Infrastructure.Data;

public sealed class ApexPostDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApexPostDbContext>
{
    public ApexPostDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ApexPostDesignTimeDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = configuration["APEXPOST_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("ApexPost")
            ?? "Server=DESKTOP-FB55L35;Database=ApexPostDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=True;Application Intent=ReadWrite;Connection Timeout=30;";
        var options = new DbContextOptionsBuilder<ApexPostDbContext>().UseSqlServer(connection).Options;
        return new ApexPostDbContext(options);
    }
}
