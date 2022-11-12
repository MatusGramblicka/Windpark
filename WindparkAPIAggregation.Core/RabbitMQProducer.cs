using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly ILogger<RabbitMQProducer> _logger;

        public RabbitMQProducer(ILogger<RabbitMQProducer> logger)
        {
            _logger = logger;
        }

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672
            };

            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            _logger.LogInformation("Connection to RabbitMQ created");

            channel.QueueDeclare("windpark", exclusive: false);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: "windpark", body: body);
            _logger.LogInformation("Data is sent to RabbitMQ");
        }
    }
}