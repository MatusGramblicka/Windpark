using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RestLibrary;
using System.Net.Http;
using System.Threading.Tasks;
using Quartz;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class WindparkClient: IWindparkClient, IJob
    {
        private readonly RestClient _restClient;
        private List<WindParkAggregationData> _windParkAggregationData = new List<WindParkAggregationData>();
        public WindparkClient(ILogger<RestClient> restLogger, HttpClient httpClient)
        {
            _restClient = new RestClient(httpClient, restLogger);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await GetData();
        }

        public async Task GetData()
        {
            var windParkCollectionData = await _restClient.CallAsync<List<WindPark>>($"api/Site", HttpMethod.Get);

            foreach (var windPark in windParkCollectionData)
            {
                var windParkId = windPark.Id;

                var windParkData = await _restClient.CallAsync<WindPark>($"api/Site/{windParkId}", HttpMethod.Get);


                _windParkAggregationData.Add(new WindParkAggregationData
                {
                    Id = windParkData.Id,
                    AggregatedTurbine = windParkData.Turbines.Select(t => new AggregatedTurbine
                    {
                        Id = t.Id,
                        CurrentProduction = new List<double> {t.CurrentProduction},
                        windspeed = new List<double> {t.Windspeed}
                    }).ToList()
                });
            }
        }

        public List<WindParkAggregationData> GetAggregatedData()
        {
            return _windParkAggregationData;
        }
    }
}