using System;
using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIAggregation.Interface
{
    public interface IWindparkClient
    {
        Task GetData();
        AggregatedData GetAggregatedDataFromMemory();
        Task<AggregatedData> GetAggregatedDataFromDb();
        void CleanAggregatedData(DateTime datetime);
    }
}