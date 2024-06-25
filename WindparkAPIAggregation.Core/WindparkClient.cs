using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts;
using WindParkAPIAggregation.Interface;

namespace WindParkAPIAggregation.Core;

public class WindParkClient : IWindParkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WindParkClient> _logger;
    private readonly IDatabaseOperation _databaseOperation;

    private const string PartialUri = "api/Site";

    public WindParkClient(HttpClient httpClient, ILogger<WindParkClient> logger, IDatabaseOperation databaseOperation)
    {
        _httpClient = httpClient;
        _logger = logger;
        _databaseOperation = databaseOperation;
    }

    //public async Task GetDataToDatabase()
    //{
    //    _logger.LogInformation("Getting windParks data");
    //    var windParkCollectionData =
    //        await _httpClient.GetFromJsonAsync<List<WindParkDto>>($"{PartialUri}");

    //    if (windParkCollectionData == null)
    //    {
    //        return;
    //    }

    //    foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
    //    {
    //        var windParkData = await _httpClient.GetFromJsonAsync<WindParkDto>($"{PartialUri}/{windParkId}");

    //        if (windParkData == null)
    //        {
    //            continue;
    //        }

    //        await _databaseOperation.SaveToDb(windParkData);
    //    }
    //}

    public async Task GetWindParkData()
    {
        _logger.LogInformation("Getting windParks data");
        var rnd = new Random();

        var windParkCollectionData = new List<WindParkDto>
        {
            new() {Id = 1},
            new() {Id = 2}
        };

        if (windParkCollectionData == null)
        {
            return;
        }

        //foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
        //{
            var windParkData = new WindParkDto
            {
                Turbines = new List<TurbineDto>
                {
                    new()
                    {
                        Id = 1,
                        CurrentProduction = rnd.Next(1000),
                        WindSpeed = rnd.Next(100)
                    },
                    new()
                    {
                        Id = 2,
                        CurrentProduction = rnd.Next(1000),
                        WindSpeed = rnd.Next(100)
                    }
                }
            };

            await _databaseOperation.SaveToDb(windParkData);
        //}
    }
}