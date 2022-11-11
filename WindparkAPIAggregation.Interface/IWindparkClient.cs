using System.Collections.Generic;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIAggregation.Interface
{
    public interface IWindparkClient
    {
        Task GetData();
        List<WindParkAggregationData> GetAggregatedData();
    }
}