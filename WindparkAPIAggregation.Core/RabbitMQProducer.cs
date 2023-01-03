using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using Polly;
using RabbitMQ.Client.Exceptions;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core;

public class RabbitMqProducer : IMessageProducer
{
    private readonly ILogger<RabbitMqProducer> _logger;

    private readonly RabbitMqConfiguration _rabbitMqConfiguration;
    private IConnection _connection;

    private const int RetryCount = 3;

    public RabbitMqProducer(ILogger<RabbitMqProducer> logger, IOptions<RabbitMqConfiguration> rabbitMqConfiguration)
    {
        _logger = logger;
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
    }

    public void SendMessage<T>(T message) where T: class
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqConfiguration.HostName,
            Port = _rabbitMqConfiguration.Port
        };

        var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>()
            .WaitAndRetry(RetryCount, op => TimeSpan.FromSeconds(Math.Pow(2, op)),
                (ex, time) => { _logger.LogInformation("Couldn't connect to RabbitMQ server."); });

        policy.Execute(() => { _connection = factory.CreateConnection(); });
        
        using var channel = _connection.CreateModel();
        _logger.LogDebug("Connection to RabbitMQ created");

        channel.QueueDeclare(_rabbitMqConfiguration.Queue, exclusive: false, autoDelete: true);

        var jsonString = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(jsonString);

        channel.BasicPublish(exchange: "", routingKey: _rabbitMqConfiguration.RoutingKey, body: body);
        _logger.LogDebug("Data is sent to RabbitMQ");
    }
}