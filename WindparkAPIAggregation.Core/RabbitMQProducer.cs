using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class RabbitMQProducer : IMessageProducer
    {
        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare("windpark", exclusive: false);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: "windpark", body: body);
        }
    }
}