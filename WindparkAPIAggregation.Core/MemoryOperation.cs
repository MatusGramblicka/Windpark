using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core;

public class MemoryOperation : IMemoryOperation
{
    private readonly ILogger<MemoryOperation> _logger;

    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private readonly WindParkAggregationPersistor _windParkAggregationPersistor;

    public MemoryOperation(WindParkAggregationPersistor windParkAggregationPersistor,
        ILogger<MemoryOperation> logger)
    {
        _windParkAggregationPersistor = windParkAggregationPersistor;
        _logger = logger;
    }

    public async Task SaveToMemory(WindParkDto windParkData)
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
                AggregatedTurbineData = Aggregator.Aggregate(windPark)
            });
        }

        return new AggregatedData
        {
            WindParkAggregatedData = windParkAggregated,
            DataSource = DataSource.InMemorySource
        };
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
}