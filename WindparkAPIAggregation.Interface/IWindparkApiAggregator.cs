using System.Threading.Tasks;

namespace WindparkAPIAggregation.Interface
{
    public interface IWindparkApiAggregator
    {
        Task SendDataToRabbitMq();
    }
}