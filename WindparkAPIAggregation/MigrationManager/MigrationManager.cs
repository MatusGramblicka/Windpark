using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using WindParkAPIAggregation.Repository;

namespace WindParkAPIAggregation.MigrationManager;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
        
        try
        {
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Migration was not successful: {ex}");
            throw;
        }

        return host;
    }
}