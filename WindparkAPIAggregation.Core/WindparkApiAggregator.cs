using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core;

public class WindparkApiAggregator : IWindparkApiAggregator, IJob
{
    private readonly IWindparkClient _windparkClient;
    private readonly IMessageProducer _messagePublisher;
    private readonly ILogger<WindparkApiAggregator> _logger;

    public WindparkApiAggregator(IWindparkClient windparkClient, IMessageProducer messagePublisher,
        ILogger<WindparkApiAggregator> logger)
    {
        _windparkClient = windparkClient;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await SendDataToRabbitMq();
    }

    public async Task SendDataToRabbitMq()
    {
        var aggregatedData = _windparkClient.GetAggregatedDataFromMemory();

        if (aggregatedData == null || aggregatedData.WindParkAggregatedData.Any())
        {
            _logger.LogDebug("Sending GetAggregatedData to rabbit");
            _messagePublisher.SendMessage(aggregatedData);
            await _windparkClient.CleanAggregatedData(DateTime.Now);
        }

        var aggregatedDataFromDb = await _windparkClient.GetAggregatedDataFromDb();

        if (aggregatedData == null || aggregatedData.WindParkAggregatedData.Any())
        {
            _logger.LogDebug("Sending GetAggregatedDataFromDb to rabbit");
            _messagePublisher.SendMessage(aggregatedDataFromDb);
        }
    }
}