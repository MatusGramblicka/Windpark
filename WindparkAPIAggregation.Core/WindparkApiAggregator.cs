using System.Linq;
using Quartz;
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

        public async Task SendDataToRabbitMq()
        {
            //await _windparkClient.GetData();

            var aggregatedData = _windparkClient.GetAggregatedData();

            if (aggregatedData == null || aggregatedData.Any())
            {
                // todo aggregate data better, flat the structure

                _messagePublisher.SendMessage(aggregatedData);
            }
        }
    }
}