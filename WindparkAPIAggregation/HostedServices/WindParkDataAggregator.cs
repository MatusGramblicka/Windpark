using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts;
using WindParkAPIAggregation.Core;
using WindParkAPIAggregation.Interface;

namespace WindParkAPIAggregation.HostedServices;

public class WindParkDataAggregator : BackgroundService
{
    private readonly WindParkIntervalConfiguration _windParkIntervalConfiguration;

    private readonly IMessageProducer _messagePublisher;
    private readonly ILogger<WindParkApiAggregator> _logger;
    private readonly IDatabaseOperation _databaseOperation;

    public WindParkDataAggregator(IMessageProducer messagePublisher,
        ILogger<WindParkApiAggregator> logger, IDatabaseOperation databaseOperation,
        IOptions<WindParkIntervalConfiguration> windParkIntervalConfiguration)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
        _databaseOperation = databaseOperation;
        _windParkIntervalConfiguration = windParkIntervalConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(
            TimeSpan.FromMinutes(_windParkIntervalConfiguration.WindParkAggregationFrequencyMinutes));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var aggregatedDataFromDb = await _databaseOperation.GetAggregatedDataFromDb();

            if (aggregatedDataFromDb == null || aggregatedDataFromDb.WindParkAggregatedData.Any())
            {
                _logger.LogDebug("Sending GetAggregatedDataFromDb to rabbit");
                _messagePublisher.SendMessage(aggregatedDataFromDb);
            }
        }
    }
}