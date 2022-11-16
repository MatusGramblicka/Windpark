using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIConsumer.HostedServices;

public class ReceiverService : BackgroundService
{
    private readonly ILogger<ReceiverService> _logger;

    private readonly RabbitMqConfiguration _rabbitMqConfiguration;

    private IConnection _connection;
    private readonly IModel _channel;

    private const int RetryCount = 3;

    public ReceiverService(ILogger<ReceiverService> logger,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration)
    {
        _logger = logger;
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqConfiguration.HostName,
            Port = _rabbitMqConfiguration.Port
        };

        var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>()
            .WaitAndRetry(RetryCount, op => TimeSpan.FromSeconds(Math.Pow(2, op)),
                (ex, time) => { _logger.LogInformation("Couldn't connect to RabbitMQ server."); });

        policy.Execute(() => { _connection = factory.CreateConnection(); });

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: _rabbitMqConfiguration.Queue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            _channel.Dispose();
            _connection.Dispose();
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Message received from RabbitMQ: {message}");

            var windParkData = JsonConvert.DeserializeObject<List<WindParkAggregated>>(message);

            if (windParkData != null)
            {
                // do your stuff with the data ...
            }
        };

        _channel.BasicConsume(queue: _rabbitMqConfiguration.Queue, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}