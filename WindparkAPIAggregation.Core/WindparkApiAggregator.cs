using Microsoft.Extensions.Logging;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using WindParkAPIAggregation.Interface;

namespace WindParkAPIAggregation.Core;

public class WindParkApiAggregator : IWindParkApiAggregator, IJob
{
    private readonly IMessageProducer _messagePublisher;
    private readonly ILogger<WindParkApiAggregator> _logger;

    private readonly IDatabaseOperation _databaseOperation;

    public WindParkApiAggregator(IMessageProducer messagePublisher,
        ILogger<WindParkApiAggregator> logger, IDatabaseOperation databaseOperation)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
        _databaseOperation = databaseOperation;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await SendDataToRabbitMq();
    }


    // not registered, do not use
    public async Task SendDataToRabbitMq()
    {
        var aggregatedDataFromDb = await _databaseOperation.GetAggregatedDataFromDb();

        if (aggregatedDataFromDb == null || aggregatedDataFromDb.WindParkAggregatedData.Any())
        {
            _logger.LogDebug("Sending GetAggregatedDataFromDb to rabbit");
            _messagePublisher.SendMessage(aggregatedDataFromDb);
        }
    }
}