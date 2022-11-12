using Microsoft.Extensions.Logging;
using Quartz;
using RestLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class WindparkClient : IWindparkClient, IJob
    {
        private readonly RestClient _restClient;
        private readonly ILogger<WindparkClient> _logger;

        private readonly WindParkAggregationPersistor _windParkAggregationPersistor;

        public WindparkClient(ILogger<RestClient> restLogger, HttpClient httpClient,
            WindParkAggregationPersistor windParkAggregationPersistor, ILogger<WindparkClient> logger)
        {
            _windParkAggregationPersistor = windParkAggregationPersistor;
            _logger = logger;
            _restClient = new RestClient(httpClient, restLogger);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await GetData();
        }

        public async Task GetData()
        {
            _logger.LogInformation("Getting windparks data");
            var windParkCollectionData = await _restClient.CallAsync<List<WindPark>>($"api/Site", HttpMethod.Get);

            foreach (var windParkId in windParkCollectionData.Select(windPark => windPark.Id))
            {
                var windParkData = await _restClient.CallAsync<WindPark>($"api/Site/{windParkId}", HttpMethod.Get);

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
            _windParkAggregationPersistor.WindParkAggregationData.Clear();
        }
    }
}