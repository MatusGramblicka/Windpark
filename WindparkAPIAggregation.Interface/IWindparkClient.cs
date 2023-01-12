using System.Threading.Tasks;

namespace WindparkAPIAggregation.Interface;

public interface IWindparkClient
{
    Task GetData();
}