using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;
using WindparkAPIAggregation.Repository;

namespace WindparkAPIAggregation.Core;

public class WindparkClient : IWindparkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WindparkClient> _logger;
    private readonly AppDbContext _context;

    private readonly WindParkAggregationPersistor _windParkAggregationPersistor;

    public WindparkClient(HttpClient httpClient, WindParkAggregationPersistor windParkAggregationPersistor,
        ILogger<WindparkClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _httpClient = httpClient;
        _windParkAggregationPersistor = windParkAggregationPersistor;
        _logger = logger;

        var scope = serviceScopeFactory.CreateScope();
        _context = scope.ServiceProvider.GetService<AppDbContext>();
    }

    public async Task GetData()
    {
        _logger.LogInformation("Getting windparks data");
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
            SaveToMemory(windParkData);
        }
    }

    public AggregatedData GetAggregatedDataFromMemory()
    {
        var windParksGroup =
            _windParkAggregationPersistor.WindParkAggregationData.GroupBy(w => w.WindParkNumber);
        var windParkAggregated = new List<WindParkAggregated>();

        foreach (var windPark in windParksGroup)
        {
            var aggregatedTurbineFlattened = windPark.SelectMany(a => a.Turbines).ToList();
            var turbinesGroups = aggregatedTurbineFlattened.GroupBy(w => w.TurbineNumber).ToList();

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

            windParkAggregated.Add(new WindParkAggregated
            {
                WindParkId = windPark.Key,
                AggregatedTurbineData = aggregatedTurbinesData
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
            var aggregatedTurbineFlattened = windPark.SelectMany(a => a.Turbines).ToList();
            var turbinesGroups = aggregatedTurbineFlattened.GroupBy(w => w.TurbineNumber).ToList();

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

            windParkAggregated.Add(new WindParkAggregated
            {
                WindParkId = windPark.Key,
                AggregatedTurbineData = aggregatedTurbinesData
            });
        }

        await CleanAggregatedDataFromDb(windParkDb);

        return new AggregatedData
        {
            WindParkAggregatedData = windParkAggregated,
            DataSource = DataSource.SqlServer
        };
    }

    public async Task CleanAggregatedDataFromDb(IIncludableQueryable<WindPark, ICollection<Turbine>> windParks)
    {
        foreach (var windPark in windParks)
        {
            var aggregatedTurbineFlattenedIds = windPark.Turbines.Select(t => t.Id).ToList();
            _logger.LogInformation($"count db before {aggregatedTurbineFlattenedIds.Count}");

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

    public void CleanAggregatedData(DateTime datetime)
    {
        _logger.LogInformation($"count before {_windParkAggregationPersistor.WindParkAggregationData.Count}");
        var windParksForDeletion =
            _windParkAggregationPersistor.WindParkAggregationData.Where(w => w.DateAdded < datetime).ToList();
        foreach (var windParkForDeletion in windParksForDeletion)
        {
            _windParkAggregationPersistor.WindParkAggregationData.Remove(windParkForDeletion);
        }
        _logger.LogInformation($"count after {_windParkAggregationPersistor.WindParkAggregationData.Count}");
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

    private void SaveToMemory(WindParkDto windParkData)
    {
        _windParkAggregationPersistor.WindParkAggregationData.Add(new WindPark
        {
            WindParkNumber = windParkData.Id,
            Turbines = windParkData.Turbines.Select(t => new Turbine
            {
                TurbineNumber = t.Id,
                CurrentProduction = t.CurrentProduction,
                WindSpeed = t.WindSpeed
            }).ToList(),
            DateAdded = DateTime.Now
        });
    }
}