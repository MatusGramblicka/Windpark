using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.HostedServices;

public class RunScheduler : BackgroundService
{
    private readonly IWindparkClient _windParkClient;

    private readonly WindparkIntervalConfiguration _windParkIntervalConfiguration;

    public RunScheduler(IOptions<WindparkIntervalConfiguration> windParkIntervalConfiguration,
        IWindparkClient windparkClient)
    {
        _windParkClient = windparkClient;
        _windParkIntervalConfiguration = windParkIntervalConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_windParkIntervalConfiguration.WindparkApiFrequencySeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _windParkClient.GetData();
        }
    }
}