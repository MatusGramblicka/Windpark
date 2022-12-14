using Microsoft.EntityFrameworkCore;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIAggregation.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WindPark> WindPark { get; set; }
}