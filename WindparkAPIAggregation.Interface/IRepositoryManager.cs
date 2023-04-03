using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts.Models;

namespace WindParkAPIAggregation.Interface;

public interface IRepositoryManager
{
    IIncludableQueryable<WindPark,ICollection<Turbine>> GetWindParks();
    void AddWindParkAsync(WindPark windPark);
    Task SaveAsync();
}