using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.HostedServices
{
    public class RunScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWindparkClient _windparkClient;

        private readonly WindparkIntervalConfiguration _windparkIntervalConfiguration;

        public RunScheduler(IServiceScopeFactory serviceScopeFactory,
            IOptions<WindparkIntervalConfiguration> windparkIntervalConfiguration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _windparkIntervalConfiguration = windparkIntervalConfiguration.Value;
            var scope = _serviceScopeFactory.CreateScope();
            _windparkClient = scope.ServiceProvider.GetRequiredService<IWindparkClient>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Option 1
            while (!stoppingToken.IsCancellationRequested)
            {
                await _windparkClient.GetData();
                await Task.Delay(
                    TimeSpan.FromSeconds(_windparkIntervalConfiguration.WindparkApiFrequencySeconds),
                    stoppingToken);
            }

            //// Option 2 (.NET 6)
            //var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
            //while (await timer.WaitForNextTickAsync(stoppingToken))
            //{
            //    // do async work
            //    // ...as above
            //}
        }
    }
}