using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class WindparkClient : IWindparkClient, IJob
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WindparkClient> _logger;

        private readonly WindParkAggregationPersistor _windParkAggregationPersistor;

        public WindparkClient(HttpClient httpClient,
            WindParkAggregationPersistor windParkAggregationPersistor, ILogger<WindparkClient> logger)
        {
            _httpClient = httpClient;
            _windParkAggregationPersistor = windParkAggregationPersistor;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await GetData();
        }

        public async Task GetData()
        {
            _logger.LogInformation("Getting windparks data");
            var windParkCollectionData =
                await _httpClient.GetFromJsonAsync<List<WindPark>>($"api/Site");

            if (windParkCollectionData == null)
            {
                return;
            }

            foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
            {
                var windParkData = await _httpClient.GetFromJsonAsync<WindPark>($"api/Site/{windParkId}");

                if (windParkData == null)
                {
                    continue;
                }

                _windParkAggregationPersistor.WindParkAggregationData.Add(new WindParkAggregationData
                {
                    WindParkId = windParkData.Id,
                    AggregatedTurbine = windParkData.Turbines.Select(t => new AggregatedTurbine
                    {
                        TurbineId = t.Id,
                        CurrentProduction = t.CurrentProduction,
                        WindSpeed = t.Windspeed
                    }).ToList()
                });
            }
        }

        public List<WindParkAggregated> GetAggregatedData()
        {
            var windParksGroup =
                _windParkAggregationPersistor.WindParkAggregationData.GroupBy(w => w.WindParkId);
            var windParkAggregated = new List<WindParkAggregated>();

            foreach (var windPark in windParksGroup)
            {
                var aggregatedTurbineFlattened = windPark.SelectMany(a => a.AggregatedTurbine).ToList();
                var turbinesGroups = aggregatedTurbineFlattened.GroupBy(w => w.TurbineId).ToList();

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

            return windParkAggregated;
        }

        public void CleanAggregatedData()
        {
            // _windParkAggregationPersistor should be accessed only exclusively,
            // when writing and then deleting,
            // since it can happen that it deletes data which was not send
            _windParkAggregationPersistor.WindParkAggregationData.Clear();
        }
    }
}