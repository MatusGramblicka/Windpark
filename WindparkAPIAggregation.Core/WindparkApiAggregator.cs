﻿using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using WindParkAPIAggregation.Interface;

namespace WindParkAPIAggregation.Core;

public class WindParkApiAggregator : IWindParkApiAggregator, IJob
{
    private readonly IMessageProducer _messagePublisher;
    private readonly ILogger<WindParkApiAggregator> _logger;
    private readonly IMemoryOperation _memoryOperation;
    private readonly IDatabaseOperation _databaseOperation;

    public WindParkApiAggregator(IMessageProducer messagePublisher,
        ILogger<WindParkApiAggregator> logger, IMemoryOperation memoryOperation, IDatabaseOperation databaseOperation)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
        _memoryOperation = memoryOperation;
        _databaseOperation = databaseOperation;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await SendDataToRabbitMq();
    }

    public async Task SendDataToRabbitMq()
    {
        var aggregatedData = _memoryOperation.GetAggregatedDataFromMemory();

        if (aggregatedData == null || aggregatedData.WindParkAggregatedData.Any())
        {
            _logger.LogDebug("Sending GetAggregatedData to rabbit");
            _messagePublisher.SendMessage(aggregatedData);
            await _memoryOperation.CleanAggregatedData(DateTime.Now);
        }

        var aggregatedDataFromDb = await _databaseOperation.GetAggregatedDataFromDb();

        if (aggregatedData == null || aggregatedData.WindParkAggregatedData.Any())
        {
            _logger.LogDebug("Sending GetAggregatedDataFromDb to rabbit");
            _messagePublisher.SendMessage(aggregatedDataFromDb);
        }
    }
}