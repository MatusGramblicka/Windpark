using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;
using WindparkAPIAggregation.Repository;

namespace WindparkAPIAggregation.Core;

public class WindParkClient : IWindparkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WindParkClient> _logger;
    private readonly AppDbContext _context;

    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private readonly WindParkAggregationPersistor _windParkAggregationPersistor;

    public WindParkClient(HttpClient httpClient, WindParkAggregationPersistor windParkAggregationPersistor,
        ILogger<WindParkClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _httpClient = httpClient;
        _windParkAggregationPersistor = windParkAggregationPersistor;
        _logger = logger;

        var scope = serviceScopeFactory.CreateScope();
        _context = scope.ServiceProvider.GetService<AppDbContext>();
    }

    public async Task GetData()
    {
        _logger.LogInformation("Getting windParks data");
        var windParkCollectionData =
            await _httpClient.GetFromJsonAsync<List<WindParkDto>>($"api/Site");

        if (windParkCollectionData == null)
        {
            return;
        }

        foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
        {
            var windParkData = await _httpClient.GetFromJsonAsync<WindParkDto>($"api/Site/{windParkId}");

            if (windParkData == null)
            {
                continue;
            }

            await SaveToDb(windParkData);
            await SaveToMemory(windParkData);
        }
    }

    public AggregatedData GetAggregatedDataFromMemory()
    {
        var windParksGroup =
            _windParkAggregationPersistor.WindParkAggregationData.GroupBy(w => w.WindParkNumber);
        var windParkAggregated = new List<WindParkAggregated>();

        foreach (var windPark in windParksGroup)
        {
            windParkAggregated.Add(new WindParkAggregated
            {
                WindParkId = windPark.Key,
                AggregatedTurbineData = Aggregate(windPark)
            });
        }

        return new AggregatedData
        {
            WindParkAggregatedData = windParkAggregated,
            DataSource = DataSource.InMemorySource
        };
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
                AggregatedTurbineData = /*aggregatedTurbinesData*/Aggregate(windPark)
            });
        }

        await CleanAggregatedDataFromDb(windParkDb);

        return new AggregatedData
        {
            WindParkAggregatedData = windParkAggregated,
            DataSource = DataSource.SqlServer
        };
    }

    private static List<AggregatedTurbineData> Aggregate(IGrouping<int, WindPark> windPark)
    {
        var aggregatedTurbineFlattened = windPark.SelectMany(a => a.Turbines);
        var turbinesGroups = aggregatedTurbineFlattened.GroupBy(w => w.TurbineNumber);

        var aggregatedTurbinesData = new List<AggregatedTurbineData>();

        foreach (var turbines in turbinesGroups)
        {
            var aggregatedTurbineData = new AggregatedTurbineData
            {
                TurbineId = turbines.Key,
                WindSpeedCurrentProductionAggregation =
                    turbines.Select(t => (t.WindSpeed, t.CurrentProduction)).ToList()
            };

            aggregatedTurbinesData.Add(aggregatedTurbineData);
        }

        return aggregatedTurbinesData;
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

    public async Task CleanAggregatedData(DateTime datetime)
    {
        _logger.LogInformation(
            $"count before memory deletion: {_windParkAggregationPersistor.WindParkAggregationData.Count}");
        var windParksForDeletion =
            _windParkAggregationPersistor.WindParkAggregationData.Where(w => w.DateAdded < datetime).ToList();
        await ExecutionDecorator.ExecuteAction(windParksForDeletion, _ =>
        {
            foreach (var windParkForDeletion in windParksForDeletion)
            {
                _windParkAggregationPersistor.WindParkAggregationData.Remove(windParkForDeletion);
            }

            return Task.CompletedTask;
        }, _logger, _stateLock);
        _logger.LogInformation(
            $"count after memory deletion:{_windParkAggregationPersistor.WindParkAggregationData.Count}");
    }

    private async Task SaveToDb(WindParkDto windParkData)
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

    private async Task SaveToMemory(WindParkDto windParkData)
    {
        await ExecutionDecorator.ExecuteAction(windParkData, r =>
        {
            _windParkAggregationPersistor.WindParkAggregationData.Add(new WindPark
            {
                WindParkNumber = r.Id,
                Turbines = r.Turbines.Select(t => new Turbine
                {
                    TurbineNumber = t.Id,
                    CurrentProduction = t.CurrentProduction,
                    WindSpeed = t.WindSpeed
                }).ToList(),
                DateAdded = DateTime.Now
            });

            return Task.CompletedTask;
        }, _logger, _stateLock);
    }
}