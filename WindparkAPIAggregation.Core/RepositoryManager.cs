using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts.Models;
using WindParkAPIAggregation.Interface;
using WindParkAPIAggregation.Repository;

namespace WindParkAPIAggregation.Core;

public class RepositoryManager : IRepositoryManager
{
    private readonly AppDbContext _context;

    public RepositoryManager(AppDbContext context)
    {
        _context = context;
    }

    public void AddWindParkAsync(WindPark windPark)
    {
        _context.WindPark.Add(windPark);
    }

    public IIncludableQueryable<WindPark, ICollection<Turbine>> GetWindParks()
    {
        return _context.WindPark.Include(w => w.Turbines);
    }

    public Task SaveAsync() => _context.SaveChangesAsync();
}