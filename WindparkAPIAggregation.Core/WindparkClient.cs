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
    private readonly IMemoryOperation _memoryOperation;
    private readonly IDatabaseOperation _databaseOperation;

    private const string PartialUri = "api/Site";

    public WindParkClient(HttpClient httpClient, ILogger<WindParkClient> logger, IMemoryOperation memoryOperation,
        IDatabaseOperation databaseOperation)
    {
        _httpClient = httpClient;
        _logger = logger;
        _memoryOperation = memoryOperation;
        _databaseOperation = databaseOperation;
    }

    public async Task GetData()
    {
        _logger.LogInformation("Getting windParks data");
        var windParkCollectionData =
            await _httpClient.GetFromJsonAsync<List<WindParkDto>>($"{PartialUri}");

        if (windParkCollectionData == null)
        {
            return;
        }

        foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
        {
            var windParkData = await _httpClient.GetFromJsonAsync<WindParkDto>($"{PartialUri}/{windParkId}");

            if (windParkData == null)
            {
                continue;
            }

            await _databaseOperation.SaveToDb(windParkData);
            await _memoryOperation.SaveToMemory(windParkData);
        }
    }
}