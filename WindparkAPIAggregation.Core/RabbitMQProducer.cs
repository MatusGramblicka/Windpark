using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly ILogger<RabbitMQProducer> _logger;

        private readonly RabbitMqConfiguration _rabbitMqConfiguration;

        public RabbitMQProducer(ILogger<RabbitMQProducer> logger, IOptions<RabbitMqConfiguration> rabbitMqConfiguration)
        {
            _logger = logger;
            _rabbitMqConfiguration = rabbitMqConfiguration.Value;
        }

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqConfiguration.HostName,
                Port = _rabbitMqConfiguration.Port
            };

            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            _logger.LogInformation("Connection to RabbitMQ created");

            channel.QueueDeclare(_rabbitMqConfiguration.Queue, exclusive: false, autoDelete: true);

            var jsonString = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            channel.BasicPublish(exchange: "", routingKey: _rabbitMqConfiguration.RoutingKey, body: body);
            _logger.LogInformation("Data is sent to RabbitMQ");
        }
    }
}