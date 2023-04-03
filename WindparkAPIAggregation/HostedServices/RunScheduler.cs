using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts;
using WindParkAPIAggregation.Interface;

namespace WindParkAPIAggregation.HostedServices;

public class RunScheduler : BackgroundService
{
    private readonly IWindParkClient _windParkClient;

    private readonly WindParkIntervalConfiguration _windParkIntervalConfiguration;

    public RunScheduler(IOptions<WindParkIntervalConfiguration> windParkIntervalConfiguration,
        IWindParkClient windParkClient)
    {
        _windParkClient = windParkClient;
        _windParkIntervalConfiguration = windParkIntervalConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_windParkIntervalConfiguration.WindParkApiFrequencySeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _windParkClient.GetData();
        }
    }
}