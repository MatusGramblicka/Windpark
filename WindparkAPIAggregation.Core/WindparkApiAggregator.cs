using Quartz;
using System.Linq;
using System.Threading.Tasks;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Core
{
    public class WindparkApiAggregator : IWindparkApiAggregator, IJob
    {
        private readonly IWindparkClient _windparkClient;
        private readonly IMessageProducer _messagePublisher;

        public WindparkApiAggregator(IWindparkClient windparkClient, IMessageProducer messagePublisher)
        {
            _windparkClient = windparkClient;
            _messagePublisher = messagePublisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await SendDataToRabbitMq();
        }

        public Task SendDataToRabbitMq()
        {
            var aggregatedData = _windparkClient.GetAggregatedData();

            if (aggregatedData == null || aggregatedData.Any())
            {
                _messagePublisher.SendMessage(aggregatedData);
                _windparkClient.CleanAggregatedData();
            }

            return Task.CompletedTask;
        }
    }
}