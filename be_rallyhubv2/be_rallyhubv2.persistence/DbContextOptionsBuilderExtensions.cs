using Microsoft.EntityFrameworkCore;

namespace be_rallyhubv2.persistence;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseAppPostgres(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        => optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
}