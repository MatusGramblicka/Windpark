using Microsoft.EntityFrameworkCore;
using WindParkAPIAggregation.Contracts.Models;

namespace WindParkAPIAggregation.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WindPark> WindPark { get; set; }
}