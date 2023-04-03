using System.Threading.Tasks;

namespace WindParkAPIAggregation.Interface;

public interface IWindParkApiAggregator
{
    Task SendDataToRabbitMq();
}