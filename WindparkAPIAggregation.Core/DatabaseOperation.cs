using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;
using WindparkAPIAggregation.Repository;

namespace WindparkAPIAggregation.Core;

public class DatabaseOperation : IDatabaseOperation
{
    private readonly ILogger<DatabaseOperation> _logger;
    private readonly AppDbContext _context;

    public DatabaseOperation(
        ILogger<DatabaseOperation> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;

        var scope = serviceScopeFactory.CreateScope();
        _context = scope.ServiceProvider.GetService<AppDbContext>();
    }

    
    public async Task<AggregatedData> GetAggregatedDataFromDb()
    {
        var windParkDb = _context.WindPark.Include(w => w.Turbines);

        var dataSerialized = JsonConvert.SerializeObject(windParkDb, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        _logger.LogInformation($"Data from DB: {dataSerialized}");

        var windParksGroup = windParkDb.GroupBy(w => w.WindParkNumber);
        var windParkAggregated = new List<WindParkAggregated>();

        foreach (var windPark in windParksGroup)
        {
            windParkAggregated.Add(new WindParkAggregated
            {
                WindParkId = windPark.Key,
                AggregatedTurbineData = Aggregator.Aggregate(windPark)
            });
        }

        await CleanAggregatedDataFromDb(windParkDb);

        return new AggregatedData
        {
            WindParkAggregatedData = windParkAggregated,
            DataSource = DataSource.SqlServer
        };
    }

    public async Task SaveToDb(WindParkDto windParkData)
    {
        var windParkDb = _context.WindPark.Include(w => w.Turbines)
            .FirstOrDefault(s => s.WindParkNumber.Equals(windParkData.Id));

        if (windParkDb == null)
        {
            _context.WindPark.Add(new WindPark
            {
                WindParkNumber = windParkData.Id,
                Turbines = windParkData.Turbines.Select(t => new Turbine
                {
                    TurbineNumber = t.Id,
                    CurrentProduction = t.CurrentProduction,
                    WindSpeed = t.WindSpeed
                }).ToList()
            });
        }
        else
        {
            foreach (var turbine in windParkData.Turbines)
            {
                windParkDb.Turbines.Add(new Turbine
                {
                    TurbineNumber = turbine.Id,
                    CurrentProduction = turbine.CurrentProduction,
                    WindSpeed = turbine.WindSpeed
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogInformation($"Exception when saving to db {e}");
            throw;
        }
    }

    private async Task CleanAggregatedDataFromDb(IIncludableQueryable<WindPark, ICollection<Turbine>> windParks)
    {
        foreach (var windPark in windParks)
        {
            var aggregatedTurbineFlattenedIds = windPark.Turbines.Select(t => t.Id).ToList();
            _logger.LogInformation($"count db elements before deletion: {aggregatedTurbineFlattenedIds.Count}");

            foreach (var aggregatedTurbineFlattenedId in aggregatedTurbineFlattenedIds)
            {
                try
                {
                    var turbine
                        = windPark.Turbines.Single(t => t.Id == aggregatedTurbineFlattenedId);
                    windPark.Turbines.Remove(turbine);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DB removal operation failed: {ex}");
                    throw;
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}