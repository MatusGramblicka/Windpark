using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WindparkAPIAggregation.Repository;

namespace WindparkAPIAggregation.MigrationManager;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            //Log errors or do anything you think it's needed
            throw;
        }

        return host;
    }
}