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
    private readonly IWindparkClient _windparkClient;

    private readonly WindparkIntervalConfiguration _windparkIntervalConfiguration;

    public RunScheduler(IOptions<WindparkIntervalConfiguration> windparkIntervalConfiguration,
        IWindparkClient windparkClient)
    {
        _windparkClient = windparkClient;
        _windparkIntervalConfiguration = windparkIntervalConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_windparkIntervalConfiguration.WindparkApiFrequencySeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _windparkClient.GetData();
        }
    }
}