using System.Threading.Tasks;
using System;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIAggregation.Interface;

public interface IMemoryOperation
{
    AggregatedData GetAggregatedDataFromMemory();
    Task CleanAggregatedData(DateTime datetime);
    Task SaveToMemory(WindParkDto windParkData);
}