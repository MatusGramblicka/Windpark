using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.HostedServices
{
    public class RunScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWindparkClient _windparkClient;

        public RunScheduler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var scope = _serviceScopeFactory.CreateScope();
            _windparkClient = scope.ServiceProvider.GetRequiredService<IWindparkClient>(); 
        } 

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Option 1
            while (!stoppingToken.IsCancellationRequested)
            {
                await _windparkClient.GetData();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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