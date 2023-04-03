using System.Threading.Tasks;
using System;
using WindParkAPIAggregation.Contracts;

namespace WindParkAPIAggregation.Interface;

public interface IMemoryOperation
{
    AggregatedData GetAggregatedDataFromMemory();
    Task CleanAggregatedData(DateTime datetime);
    Task SaveToMemory(WindParkDto windParkData);
}