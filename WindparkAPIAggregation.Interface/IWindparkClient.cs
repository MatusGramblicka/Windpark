using System.Threading.Tasks;

namespace WindParkAPIAggregation.Interface;

public interface IWindParkClient
{
    Task GetWindParkData();
}