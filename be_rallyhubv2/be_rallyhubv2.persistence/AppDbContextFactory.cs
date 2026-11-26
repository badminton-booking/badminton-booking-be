using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace be_rallyhubv2.persistence;

public sealed class AppDbContextFactory: IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DefaultConnectionStringName =  "Host=localhost;Port=5432;Database=rallyhubv2;Username=postgres;Password=postgres";
    
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? DefaultConnectionStringName;

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseAppPostgres(connectionString);
        
        return new AppDbContext(builder.Options);
    }
}